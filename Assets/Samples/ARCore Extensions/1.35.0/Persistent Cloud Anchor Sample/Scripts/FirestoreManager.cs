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

    public async void addAnchor(string name, string anchor_id, string anchor_type ) 
    { 
        AnchorData anchor = new AnchorData
        {
            Name = name;
            AnchorID = anchor_id;
            AnchorType = anchor_type;
        };

        try {
            DocumentReference addedDocRef = await db.Collection("anchor").AddAsync(anchor);
        } catch (Exception e){
            Debug.Log(e);
        } 

    }

    public async void addPair(string anchor1_id, string anchor2_id, ) 
    { 
        Pair pair = new Pair
        {
            Anchor1ID = anchor1_id;
            Anchor2ID = anchor2_id;
        };

        try {
            DocumentReference addedDocRef = await db.Collection("pair").AddAsync(pair);
        } catch (Exception e){
            Debug.Log(e);
        } 
    }

    public async void getAnchors() 
    {
        try
        {
            List<AnchorData> allAnchors = new List<AnchorData>();
            this.db.Collection("anchor").GetSnapshotAsync().ContinueWithOnMainThread(task =>
            {
                foreach(var oneAnchor in task.Result)
                {
                    AnchorData anchor = oneShop.ConvertTo<AnchorData>();
                    allAnchors.Add(anchor);
                }
                return allAnchors;
            });

        }
        catch (Exception error)
        {
            Debug.LogError(error);
        }
    }

    public async void getPairs() 
    {
        try
        {
            List<PairData> allPairs = new List<PairData>();
            this.db.Collection("pair").GetSnapshotAsync().ContinueWithOnMainThread(task =>
            {
                foreach(var oneAnchor in task.Result)
                {
                    PairData pair = oneShop.ConvertTo<PairData>();
                    allPairs.Add(pair);
                }
                return allPairs;
            });

        }
        catch (Exception error)
        {
            Debug.LogError(error);
        }
    }

}