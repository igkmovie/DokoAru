using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase.Firestore;
using System.Linq;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
public interface ISnapshot
{
}
[FirestoreData] public interface IFirestoredata
{
}
public delegate void EventListenerHandler(string document, Dictionary<string, object> dict);
public delegate void EventListenersHandler(List<Dictionary<string, object>> dicts);
public interface IFireStoreManager
{
    //リスナー
    event EventListenerHandler ListenerHandler;
    void SetListenerHandler(string document, string collectiton);
    void SetListenerHandlers(string collectiton);
    void SetListenerHandlers(string property, object value, string collectiton);
    
    void StoptListener(string key); //keyはDocumentIDもしくは collectiton名、もしくはproperty名
    //データ取得
    UniTask<Dictionary<string, object>> GetDocumetAsync(string document, string collection);
    UniTask<ISnapshot> GetISnapshotDocumetAsync<ISnapshot>(string document, string collection);
    UniTask<List<Dictionary<string, object>>> GetEqualToDocumetsAsync(string property, object value, string collection);
    UniTask<List<ISnapshot>> GetEqualToDocumetsAsync<ISnapshot>(string property, object value, string collection);
    //データ書き込み
    UniTask<bool> SetDocumetAsync(string document, string collectiton, Dictionary<string, object> dict);
    UniTask<bool> SetDocumetAsync(string document, string collectiton, IFirestoredata data);
    UniTask<bool> UpdateDocumetAsync(string document, string collection, Dictionary<string, object> dict);
    UniTask<bool> DeleteDocumetAsync(string document, string collection);

    DocumentReference GetDocumentReference(string document, string collection);
    CollectionReference GetCollectionReference(string collection);
    
}
public class FireStoreManager : IFireStoreManager
{
    private FirebaseFirestore _firestore;

    public event EventListenerHandler ListenerHandler;
    Dictionary<string, ListenerRegistration> ListenerDict = new Dictionary<string, ListenerRegistration>();

    public FireStoreManager()
    {
        _firestore = FirebaseFirestore.DefaultInstance;
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
        ListenerDict.Add(document, listen);
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
        ListenerDict.Add(collection, listen);

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
        ListenerDict.Add(collection, listen);
    }
    public void StoptListener(string key)
    {
        var listen = ListenerDict[key];
        listen.Stop();
        ListenerDict.Remove(key);
    }
    public async UniTask<Dictionary<string, object>> GetDocumetAsync(string document, string collection)
    {
        DocumentReference docRef = _firestore.Collection(collection).Document(document);
        var snapshot = await docRef.GetSnapshotAsync();
        if (!snapshot.Exists) return null;
        var dict = snapshot.ToDictionary();
        return dict;

    }
    public async UniTask<ISnapshot> GetISnapshotDocumetAsync<ISnapshot>(string document, string collection)
    {
        DocumentReference docRef = _firestore.Collection(collection).Document(document);
        var snapshot = await docRef.GetSnapshotAsync();
        var snap = snapshot.ConvertTo<ISnapshot>();
        return snap;
    }
    public async UniTask<List< Dictionary<string, object>>> GetEqualToDocumetsAsync(string property, object value,string collection)
    {
        List<Dictionary<string, object>> list = new List<Dictionary<string, object>>();
        Query capitalQuery = _firestore.Collection(collection).WhereEqualTo(property, value);
        var capitalQuerySnapshot = await capitalQuery.GetSnapshotAsync();
        foreach(DocumentSnapshot documentSnapshot in capitalQuerySnapshot.Documents)
        {
            Dictionary<string, object> dict = documentSnapshot.ToDictionary();
            list.Add(dict);
        }
        return list;
    }

    public async UniTask<List<ISnapshot>> GetEqualToDocumetsAsync<ISnapshot>(string property, object value, string collection)
    {
        List<ISnapshot> list = new List<ISnapshot>();
        Query capitalQuery = _firestore.Collection(collection).WhereEqualTo(property, value);
        var capitalQuerySnapshot = await capitalQuery.GetSnapshotAsync();
        foreach (DocumentSnapshot documentSnapshot in capitalQuerySnapshot.Documents)
        {
            ISnapshot snap = documentSnapshot.ConvertTo<ISnapshot>();
            list.Add(snap);
        }
        return list;
    }

    public async UniTask<bool> SetDocumetAsync(string document, string collectiton, Dictionary<string, object> dict)
    {
        try
        {
            DocumentReference docRef = _firestore.Collection(collectiton).Document(document);
            await docRef.SetAsync(dict);
            Debug.Log("send");
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError(e);
            return false;
            
        }
    }
    public async UniTask<bool> SetDocumetAsync(string document, string collectiton, IFirestoredata data)
    {
        try
        {
            DocumentReference docRef = _firestore.Collection(collectiton).Document(document);
            await docRef.SetAsync(data);
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError(e);
            return false;
            
        }
    }
    public async UniTask<bool> UpdateDocumetAsync(string document, string collectiton, Dictionary<string, object> dict)
    {
        try
        {
            DocumentReference docRef = _firestore.Collection(collectiton).Document(document);
            await docRef.UpdateAsync(dict);
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError(e);
            return false;
            
        }
    }
    public async UniTask<bool> DeleteDocumetAsync(string document, string collectiton)
    {
        try
        {
            DocumentReference docRef = _firestore.Collection(collectiton).Document(document);
            await docRef.DeleteAsync();
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError(e);
            return false;

        }
    }
}
