namespace WebSmokeTest.Engine
{
    public enum FormStatus
    {
        /// <summary>
        /// Form has been found and added to the list ready to process
        /// </summary>
        New,

        /// <summary>
        /// Form method was not GET or POST
        /// </summary>
        UnrecognisedMethod,

        /// <summary>
        /// An unexpected error occurred whilst processing the form
        /// </summary>
        Error,

        /// <summary>
        /// The form will not be processed as corresponding values have not been configured.
        /// </summary>
        NotProcessing,
    }
}
