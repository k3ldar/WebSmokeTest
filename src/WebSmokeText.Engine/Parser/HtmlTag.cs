﻿using System;
using System.Collections.Generic;

namespace SmokeTest.Engine
{
    public sealed class HtmlTag
    {
        public HtmlTag(in string name)
        {
            if (String.IsNullOrEmpty(name))
                throw new ArgumentNullException(nameof(name));

            Attributes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            Name = name;
        }

        /// <summary>
        /// Name of this tag
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Collection of attribute names and values for this tag
        /// </summary>
        public Dictionary<string, string> Attributes { get; private set; }

        /// <summary>
        /// True if this tag contained a trailing forward slash
        /// </summary>
        public bool TrailingSlash { get; set; }

        /// <summary>
        /// Indicates if this tag contains the specified attribute. Note that
        /// true is returned when this tag contains the attribute even when the
        /// attribute has no value
        /// </summary>
        /// <param name="name">Name of attribute to check</param>
        /// <returns>True if tag contains attribute or false otherwise</returns>
        public bool HasAttribute(string name)
        {
            return Attributes.ContainsKey(name);
        }
    };
}
