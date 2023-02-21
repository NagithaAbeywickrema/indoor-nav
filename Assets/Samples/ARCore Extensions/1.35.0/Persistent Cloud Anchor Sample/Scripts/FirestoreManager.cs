namespace Google.XR.ARCoreExtensions.Samples.PersistentCloudAnchors
{

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase.Extensions;
using Firebase.Firestore;

public class FirestoreManager : MonoBehaviour
{
    FirebaseFirestore db;

     private void Awake()
    {
        this.db = FirebaseFirestore.DefaultInstance;
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
}

}