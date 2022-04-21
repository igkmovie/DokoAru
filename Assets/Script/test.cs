using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class test : MonoBehaviour
{
    private IFireStoreManager _fireStoreManager;
    // Start is called before the first frame update
    async void Start()
    {
        _fireStoreManager = new FireStoreManager();
        _fireStoreManager.SetListenerHandler("igk@gmail.com","user");
        _fireStoreManager.ListenerHandler += listener;

    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public async void OnClick()
    {
        Dictionary<string, object> dic = new Dictionary<string, object>()
        {

            {"id", Guid.NewGuid().ToString()},

        };
        Debug.Log("OK");
        var a = await _fireStoreManager.UpdateDocumetAsync("igk@gmail.com", "user", dic);
        Debug.Log(a);
    }
    void listener(Dictionary<string, object> dics)
    {
        Debug.Log(dics["id"]);
    }
}
