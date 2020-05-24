using System;
using System.Collections.Generic;

namespace SmokeTest.Engine
{
    public sealed class FormAnalysis
    {
        #region Constructors

        public FormAnalysis()
        {
            Inputs = new List<FormInput>();
            Options = new List<FormOption>();
            Buttons = new List<FormButton>();
            TextAreas = new List<FormTextArea>();

            Id = String.Empty;
            Action = String.Empty;
            Method = String.Empty;
        }

        #endregion Constructors

        #region Properties

        public string Id { get; set; }

        public string Method { get; set; }

        public string Action { get; set; }

        public List<FormInput> Inputs { get; set; }

        public List<FormOption> Options { get; set; }

        public List<FormButton> Buttons { get; set; }

        public List<FormTextArea> TextAreas { get; set; }

        #endregion Properties

        #region Public Methods

        public void Clear()
        {
            Id = String.Empty;
            Method = String.Empty;
            Action = String.Empty;
            Inputs.Clear();
            Options.Clear();
        }

        #endregion Public Methods
    }
}
