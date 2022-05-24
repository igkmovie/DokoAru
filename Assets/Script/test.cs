using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Firebase.Firestore;

public class test : MonoBehaviour
{
    private IServerModel _fireStoreModel;
    // Start is called before the first frame update
    async void Start()
    {
        _fireStoreModel = new FireStoreModel();
        _fireStoreModel.SetListenerHandlers("user");
        _fireStoreModel.ListenerHandler += listener;
        var email = "igkworks@gmail.com";
        var password = "chiturisu";
        await _fireStoreModel.SignInWithEmailAndPasswordAsync(email, password);

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

        var a = await _fireStoreModel.GetEqualToDocumetsAsync<Mydata>("name", "kenichi","user");
        Debug.Log(a.Item2.Count);
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