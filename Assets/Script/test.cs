using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Firebase.Firestore;

public class test : MonoBehaviour
{
    private IFireStoreManager _fireStoreManager;
    // Start is called before the first frame update
    async void Start()
    {
        _fireStoreManager = new FireStoreManager();
        _fireStoreManager.SetListenerHandlers("user");
        _fireStoreManager.ListenerHandler += listener;

    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public async void OnClick()
    {
        //Dictionary<string, object> dic = new Dictionary<string, object>()
        //{

        //    {"id", Guid.NewGuid().ToString()},

        //};
        //Debug.Log("OK");
        //var a = await _fireStoreManager.UpdateDocumetAsync("igk@gmail.com", "user", dic);
        //var a = await _fireStoreManager.GetISnapshotDocumetAsync<Mydata>("igk@gmail.com", "user");

        var a = await _fireStoreManager.GetEqualToDocumetsAsync<Mydata>("name", "kenichi","user");
        Debug.Log(a.Count);
    }
    void listener(string document, Dictionary<string, object> dics)
    {
        Debug.Log(document);
    }
}
[FirestoreData]
public class Mydata : ISnapshot
{
    [FirestoreProperty]
    public string name { get; set; }
    [FirestoreProperty]
    public string mail { get; set; }

    [FirestoreProperty]
    public string id { get; set; }

}