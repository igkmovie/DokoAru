using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase.Firestore;
using System.Linq;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;

namespace yourcontents { 
}
public interface ISnapshot
{
}
[FirestoreData] public interface IFirestoredata
{
}
public delegate void EventListenerHandler(Dictionary<string, object> dict);
public delegate void EventListenersHandler(List<Dictionary<string, object>> dicts);
public interface IFireStoreManager
{
    //データ取得
    event EventListenerHandler ListenerHandler;
    event EventListenersHandler ListenersHandler;
    void SetListenerHandler(string document, string collectiton);
    void SetListenerHandlers(string collectiton);
    void SetListenerHandlers(string Document, object value, string collectiton);
    UniTask<Dictionary<string, object>> GetDocumetAsync(string document, string collection);
    UniTask<ISnapshot> GetISnapshotDocumetAsync(string document, string collection);
    UniTask<List<Dictionary<string, object>>> GetDocumetsAsync(string document, object value, string collection);
    UniTask<List<ISnapshot>> GetISnapshotDocumetsAsync(string document, object value, string collection);
    //データ書き込み
    UniTask<bool> SetDocumetAsync(string document, string collectiton, Dictionary<string, object> dict);
    UniTask<bool> SetDocumetAsync(string document, string collectiton, IFirestoredata data);
    UniTask<bool> UpdateDocumetAsync(string document, string collectiton, Dictionary<string, object> dict);
    UniTask<bool> DeleteDocumetAsync(string document, string collectiton);


}
public class FireStoreManager : IFireStoreManager
{
    private FirebaseFirestore _firestore;

    public event EventListenerHandler ListenerHandler;
    public event EventListenersHandler ListenersHandler;

    public FireStoreManager()
    {
        _firestore = FirebaseFirestore.DefaultInstance;
    }
    public void SetListenerHandler(string document, string collectiton)
    {
        DocumentReference reference = _firestore.Collection(collectiton).Document(document);

        reference.Listen(snapshot => {
            Dictionary<string, object> dic = snapshot.ToDictionary();
            Debug.Log("listener");
            ListenerHandler(dic);
        });
    }

    public void SetListenerHandlers(string collectiton)
    {
        CollectionReference reference = _firestore.Collection(collectiton);
        List<Dictionary<string, object>> dicts = new List<Dictionary<string, object>>();
        reference.Listen(snapshot => {
            foreach(DocumentSnapshot documentSnapshot in snapshot.Documents)
            {
                var dict = documentSnapshot.ToDictionary();
                dicts.Add(dict);
            }
            ListenersHandler(dicts);
        });
        
    }
    public void SetListenerHandlers(string Document, object value,   string collectiton)
    {

        Query query = _firestore.Collection(collectiton).WhereEqualTo(Document,value);
        List<Action<Dictionary<string, object>>> callbacks = new List<Action<Dictionary<string, object>>>();

        query.Listen(snapshot => {
            foreach (DocumentSnapshot documentSnapshot in snapshot.Documents)
            {
                var dict = documentSnapshot.ToDictionary();
                Action<Dictionary<string, object>> callback = dic => { };
                callback(dict);
            }
            var a = snapshot.GetChanges();
            
        });
    }
    public async UniTask<Dictionary<string, object>> GetDocumetAsync(string document, string collection)
    {
        DocumentReference docRef = _firestore.Collection(collection).Document(document);
        var snapshot = await docRef.GetSnapshotAsync();
        if (!snapshot.Exists) return null;
        var dict = snapshot.ToDictionary();
        return dict;

    }
    public async UniTask<ISnapshot> GetISnapshotDocumetAsync(string document, string collection)
    {
        DocumentReference docRef = _firestore.Collection(collection).Document(document);
        var snapshot = await docRef.GetSnapshotAsync();
        if (!snapshot.Exists) return null;
        var snap = snapshot.ConvertTo<ISnapshot>();
        return snap;
    }
    public async UniTask<List< Dictionary<string, object>>> GetDocumetsAsync(string document, object value,string collection)
    {
        List<Dictionary<string, object>> list = new List<Dictionary<string, object>>();
        Query capitalQuery = _firestore.Collection(collection).WhereEqualTo(document, value);
        var capitalQuerySnapshot = await capitalQuery.GetSnapshotAsync();
        foreach(DocumentSnapshot documentSnapshot in capitalQuerySnapshot.Documents)
        {
            Dictionary<string, object> dict = documentSnapshot.ToDictionary();
            list.Add(dict);
        }
        return list;
    }

    public async UniTask<List<ISnapshot>> GetISnapshotDocumetsAsync(string document, object value, string collection)
    {
        List<ISnapshot> list = new List<ISnapshot>();
        Query capitalQuery = _firestore.Collection(collection).WhereEqualTo(document, value);
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
