namespace Google.XR.ARCoreExtensions.Samples.PersistentCloudAnchors
{
    using Firebase.Firestore;

    [FirestoreData]
    public struct PairData 
    {
        [FirestoreProperty]
        public string Anchor1ID{get; set;}

        [FirestoreProperty]
        public string Anchor2ID{get; set;}
    }
}