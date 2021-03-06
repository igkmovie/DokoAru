using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase.Firestore;
using Firebase.Auth;
using System.Linq;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using System.Threading;

public interface ISnapshot
{
}
[FirestoreData] public interface IFirestoredata
{
}
public delegate void EventListenerHandler(string document, Dictionary<string, object> dict);
public delegate void EventListenersHandler(List<Dictionary<string, object>> dicts);
public interface IServerModel
{
    //Authentication
    Task<RESULT> CreateUserWithEmailAndPasswordAsync(string email, string password, CancellationToken ct = default);
    Task<RESULT> SignInWithEmailAndPasswordAsync(string email, string password, CancellationToken ct = default);

    //リスナー
    event EventListenerHandler ListenerHandler;
    void SetListenerHandler(string document, string collectiton);
    void SetListenerHandlers(string collectiton);
    void SetListenerHandlers(string property, object value, string collectiton);
    
    void StoptListener(string key); //keyはDocumentIDもしくは collectiton名、もしくはproperty名
                                    //データ取得
    Task<(RESULT, Dictionary<string, object>)> GetDocumetAsync(string document, string collection);
    Task<(RESULT, ISnapshot)> GetISnapshotDocumetAsync<ISnapshot>(string document, string collection);
    Task<(RESULT, List<Dictionary<string, object>>)> GetEqualToDocumetsAsync(string property, object value, string collection);
    Task<(RESULT, List<ISnapshot>)> GetEqualToDocumetsAsync<ISnapshot>(string property, object value, string collection);
    //データ書き込み
    Task<RESULT> SetDocumetAsync(string document, string collectiton, Dictionary<string, object> dict);
    Task<RESULT> SetDocumetAsync(string document, string collectiton, IFirestoredata data);
    Task<RESULT> UpdateDocumetAsync(string document, string collection, Dictionary<string, object> dict);
    Task<RESULT> DeleteDocumetAsync(string document, string collection);

    DocumentReference GetDocumentReference(string document, string collection);
    CollectionReference GetCollectionReference(string collection);
    
}
public class FireStoreModel : IServerModel
{
    private FirebaseFirestore _firestore;
    protected FirebaseAuth _auth;

    public event EventListenerHandler ListenerHandler;
    Dictionary<string, ListenerRegistration> _listenerDict = new Dictionary<string, ListenerRegistration>();

    public FireStoreModel()
    {
        _auth = FirebaseAuth.DefaultInstance;
        _firestore = FirebaseFirestore.DefaultInstance;
    }

    public async Task<RESULT> CreateUserWithEmailAndPasswordAsync(string email,string password, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        var result = RESULT.ERROR;
        try
        {
            await _auth.CreateUserWithEmailAndPasswordAsync(email, password).ContinueWith(task =>
            {
                if (task.IsCanceled)
                {
                    Debug.LogError("CreateUserWithEmailAndPasswordAsync was canceled.");
                    return;
                }
                if (task.IsFaulted)
                {
                    Debug.LogError("CreateUserWithEmailAndPasswordAsync encountered an error: " + task.Exception);
                    return;
                }
                FirebaseUser newUser = task.Result;
                Debug.LogFormat("Firebase user created successfully: {0} ({1})",
                    newUser.DisplayName, newUser.UserId);
                result = RESULT.SUCCESS;
            }, ct);
            return result;
        }
        catch (OperationCanceledException e)
        {
            // OperationCanceledExceptionをキャッチしてキャンセルをハンドリング
            Debug.Log("キャンセルされた: " + e);
            result = RESULT.ERROR;
            return result;
            throw;
        }

        
    }
    public async Task<RESULT> SignInWithEmailAndPasswordAsync(string email, string password, CancellationToken ct = default)
    {
        var result = RESULT.ERROR;
        try
        {
            ct.ThrowIfCancellationRequested();
            await _auth.SignInWithEmailAndPasswordAsync(email, password).ContinueWith(task =>
            {
                if (task.IsCanceled)
                {
                    Debug.LogError("SignInWithEmailAndPasswordAsync was canceled.");
                    return;
                }
                if (task.IsFaulted)
                {
                    Debug.LogError("SignInWithEmailAndPasswordAsync encountered an error: " + task.Exception);
                    return;
                }

                Firebase.Auth.FirebaseUser newUser = task.Result;
                Debug.LogFormat("User signed in successfully: {0} ({1})",
                    newUser.Email, newUser.UserId);
                result = RESULT.SUCCESS;
            }, ct);

            return result;
        }
        catch (OperationCanceledException e)
        {
            // OperationCanceledExceptionをキャッチしてキャンセルをハンドリング
            Debug.Log("キャンセルされた: " + e);
            result = RESULT.ERROR;
            return result;
            throw;
        }

    }

    public DocumentReference GetDocumentReference(string document, string collection)
    {
        DocumentReference reference = _firestore.Collection(collection).Document(document);
        return reference;
    }
    public CollectionReference GetCollectionReference(string collection)
    {
        CollectionReference reference = _firestore.Collection(collection);
        return reference;
    }
    public void SetListenerHandler(string document, string collection)
    {
        DocumentReference reference = _firestore.Collection(collection).Document(document);
        var listen = reference.Listen(snapshot => {
            Dictionary<string, object> dic = snapshot.ToDictionary();
            Debug.Log("listener");
            ListenerHandler(document, dic);
        });
        _listenerDict.Add(document, listen);
    }

    public void SetListenerHandlers(string collection)
    {
        CollectionReference reference = _firestore.Collection(collection);
        var listen = reference.Listen(snapshot => {
            foreach(DocumentSnapshot documentSnapshot in snapshot.Documents)
            {
                var dict = documentSnapshot.ToDictionary();
                var document = documentSnapshot.Id;
                ListenerHandler(document, dict);
            }
        });
        _listenerDict.Add(collection, listen);

    }
    public void SetListenerHandlers(string property, object value, string collection)
    {
        Query query = _firestore.Collection(collection).WhereEqualTo(property, value);
        List<Action<Dictionary<string, object>>> callbacks = new List<Action<Dictionary<string, object>>>();

        var listen = query.Listen(snapshot => {
            foreach (DocumentSnapshot documentSnapshot in snapshot.Documents)
            {
                var dict = documentSnapshot.ToDictionary();
                Action<Dictionary<string, object>> callback = dic => { };
                callback(dict);
            }
            var a = snapshot.GetChanges();
        });
        _listenerDict.Add(collection, listen);
    }
    public void StoptListener(string key)
    {
        var listen = _listenerDict[key];
        listen.Stop();
        _listenerDict.Remove(key);
    }
    public async Task<(RESULT, Dictionary<string, object>)> GetDocumetAsync(string document, string collection)
    {
        DocumentReference docRef = _firestore.Collection(collection).Document(document);
        var snapshot = await docRef.GetSnapshotAsync();
        if (!snapshot.Exists) return (RESULT.ERROR,null);
        var dict = snapshot.ToDictionary();
        return (RESULT.SUCCESS, dict);

    }
    public async Task<(RESULT, ISnapshot)> GetISnapshotDocumetAsync<ISnapshot>(string document, string collection)
    {
        try
        {
            DocumentReference docRef = _firestore.Collection(collection).Document(document);
            var snapshot = await docRef.GetSnapshotAsync();
            var snap = snapshot.ConvertTo<ISnapshot>();
            return (RESULT.SUCCESS ,snap);
        }
        catch (Exception e)
        {      
            return (RESULT.ERROR, default(ISnapshot));
        }


    }
    public async Task<(RESULT, List< Dictionary<string, object>>)> GetEqualToDocumetsAsync(string property, object value,string collection)
    {
        try
        {
            List<Dictionary<string, object>> list = new List<Dictionary<string, object>>();
            Query capitalQuery = _firestore.Collection(collection).WhereEqualTo(property, value);
            var capitalQuerySnapshot = await capitalQuery.GetSnapshotAsync();
            foreach (DocumentSnapshot documentSnapshot in capitalQuerySnapshot.Documents)
            {
                Dictionary<string, object> dict = documentSnapshot.ToDictionary();
                list.Add(dict);
            }
            return (RESULT.SUCCESS, list);
        }
        catch (Exception e)
        {
            return (RESULT.ERROR, default(List<Dictionary<string, object>>));
        }

    }

    public async Task<(RESULT, List<ISnapshot>)> GetEqualToDocumetsAsync<ISnapshot>(string property, object value, string collection)
    {
        try
        {
            List<ISnapshot> list = new List<ISnapshot>();
            Query capitalQuery = _firestore.Collection(collection).WhereEqualTo(property, value);
            var capitalQuerySnapshot = await capitalQuery.GetSnapshotAsync();
            foreach (DocumentSnapshot documentSnapshot in capitalQuerySnapshot.Documents)
            {
                ISnapshot snap = documentSnapshot.ConvertTo<ISnapshot>();
                list.Add(snap);
            }
            return (RESULT.SUCCESS, list);
        }
        catch (Exception e)
        {
            return (RESULT.ERROR, default(List<ISnapshot>));
        }


    }

    public async Task<RESULT> SetDocumetAsync(string document, string collectiton, Dictionary<string, object> dict)
    {
        var result = RESULT.NONE;
        try
        {
            DocumentReference docRef = _firestore.Collection(collectiton).Document(document);
            await docRef.SetAsync(dict);
            Debug.Log("send");
            result = RESULT.SUCCESS;
            return result;
        }
        catch (Exception e)
        {
            Debug.LogError(e);
            result = RESULT.ERROR;
            return result;
            
        }
    }
    public async Task<RESULT> SetDocumetAsync(string document, string collectiton, IFirestoredata data)
    {
        var result = RESULT.NONE;
        try
        {
            DocumentReference docRef = _firestore.Collection(collectiton).Document(document);
            await docRef.SetAsync(data);
            result = RESULT.SUCCESS;
            return result;
        }
        catch (Exception e)
        {
            Debug.LogError(e);
            result = RESULT.ERROR;
            return result;

        }
    }
    public async Task<RESULT> UpdateDocumetAsync(string document, string collectiton, Dictionary<string, object> dict)
    {
        var result = RESULT.NONE;
        try
        {
            DocumentReference docRef = _firestore.Collection(collectiton).Document(document);
            await docRef.UpdateAsync(dict);
            result = RESULT.SUCCESS;
            return result;
        }
        catch (Exception e)
        {
            Debug.LogError(e);
            result = RESULT.ERROR;
            return result;

        }
    }
    public async Task<RESULT> DeleteDocumetAsync(string document, string collectiton)
    {
        var result = RESULT.NONE;
        try
        {
            DocumentReference docRef = _firestore.Collection(collectiton).Document(document);
            await docRef.DeleteAsync();
            result = RESULT.SUCCESS;
            return result;
        }
        catch (Exception e)
        {
            Debug.LogError(e);
            result = RESULT.ERROR;
            return result;

        }
    }
}
public enum RESULT
{
    SUCCESS,
    ERROR,
    NONE
}
