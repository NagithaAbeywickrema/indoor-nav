namespace Google.XR.ARCoreExtensions.Samples.PersistentCloudAnchors
{
    using Firebase.Firestore;

    [FirestoreData]
    public struct AnchorData 
    {

        [FirestoreProperty]
        public string Name{get; set;}

        [FirestoreProperty]
        public string AnchorID{get; set;}

        [FirestoreProperty]
        public string AnchorType{get; set;}
    }
}