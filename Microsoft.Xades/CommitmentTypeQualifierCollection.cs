#region

using System.Collections;

#endregion

namespace Microsoft.Xades
{
    /// <summary>
    ///     Collection class that derives from ArrayList.  It provides the minimally
    ///     required functionality to add instances of typed classes and obtain typed
    ///     elements through a custom indexer.
    /// </summary>
    public class CommitmentTypeQualifierCollection : ArrayList
    {
        /// <summary>
        ///     New typed indexer for the collection
        /// </summary>
        /// <param name="index">Index of the object to retrieve from collection</param>
        public new CommitmentTypeQualifier this[int index]
        {
            get => (CommitmentTypeQualifier) base[index];
            set => base[index] = value;
        }

        /// <summary>
        ///     Add typed object to the collection
        /// </summary>
        /// <param name="objectToAdd">Typed object to be added to collection</param>
        /// <returns>The object that has been added to collection</returns>
        public CommitmentTypeQualifier Add(CommitmentTypeQualifier objectToAdd)
        {
            base.Add(objectToAdd);

            return objectToAdd;
        }

        /// <summary>
        ///     Add new typed object to the collection
        /// </summary>
        /// <returns>The newly created object that has been added to collection</returns>
        public CommitmentTypeQualifier Add()
        {
            return Add(new CommitmentTypeQualifier());
        }
    }
}