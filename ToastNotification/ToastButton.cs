// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

// Adapted from: https://raw.githubusercontent.com/CommunityToolkit/WindowsCommunityToolkit/master/Microsoft.Toolkit.Uwp.Notifications/Toasts/ToastButton.cs

// License:

//Copyright(c).NET Foundation and Contributors
//All rights reserved.

//Permission is hereby granted, free of charge, to any person obtaining a copy
//of this software and associated documentation files (the "Software"), to deal
//in the Software without restriction, including without limitation the rights
//to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//copies of the Software, and to permit persons to whom the Software is
//furnished to do so, subject to the following conditions:

//The above copyright notice and this permission notice shall be included in all
//copies or substantial portions of the Software.

//THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
//SOFTWARE.



using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Uno.Extras
{
    public class ToastButton
    {
        /// <summary>
        /// Gets or sets additional options relating to activation of the toast button. Unused in this implementation
        /// </summary>
        public ToastActivationOptions ActivationOptions { get; set; }
        /// <summary>
        /// Gets or sets what type of activation this button will use when clicked. Defaults to Foreground.
        /// </summary>
        public ToastActivationType ActivationType { get; set; } = ToastActivationType.Foreground;
        /// <summary>
        /// Gets app-defined string of arguments that the app can later retrieve once it is activated when the user clicks the button. Required
        /// </summary>
        public string Arguments { get; private set; }
        /// <summary>
        /// Gets the text to display on the button. Required
        /// </summary>
        public string Content { get; private set; }
        /// <summary>
        /// Gets or sets an identifier used in telemetry to identify your category of action.
        /// This should be something like "Delete", "Reply", or "Archive".
        /// </summary>
        public string HintActionId { get; set; }
        /// <summary>
        /// Gets or sets an optional image icon for the button to display (required for buttons adjacent to inputs like quick reply).
        /// </summary>
        public string ImageUri { get; set; }
        /// <summary>
        /// Gets or sets the ID of an existing ToastTextBox in order to have this button display to the right of the input, achieving a quick reply scenario.
        /// </summary>
        public string TextBoxId { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ToastButton"/> class.
        /// </summary>
        /// <param name="content">The text to display on the button.</param>
        /// <param name="arguments">App-defined string of arguments that the app can later retrieve once it is activated when the user clicks the button.</param>
        public ToastButton(string content, string arguments)
        {
            if (content == null)
            {
                throw new ArgumentNullException(nameof(content));
            }

            if (arguments == null)
            {
                throw new ArgumentNullException(nameof(arguments));
            }

            Content = content;
            Arguments = arguments;

            UsingCustomArguments = arguments.Length > 0;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ToastButton"/> class.
        /// </summary>
        public ToastButton()
        {
            // Arguments are required (we'll initialize to empty string which is fine).
            Arguments = string.Empty;
        }

        /// <summary>
        /// Adds a key (without value) to the activation arguments that will be returned when the toast notification or its buttons are clicked.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>The current instance of <see cref="ToastButton"/></returns>
        public ToastButton AddArgument(string key)
        {
            return AddArgumentHelper(key, null);
        }

        /// <summary>
        /// Adds a key/value to the activation arguments that will be returned when the toast notification or its buttons are clicked.
        /// </summary>
        /// <param name="key">The key for this value.</param>
        /// <param name="value">The value itself.</param>
        /// <returns>The current instance of <see cref="ToastButton"/></returns>
#if WINRT
        [Windows.Foundation.Metadata.DefaultOverload]
        [return: System.Runtime.InteropServices.WindowsRuntime.ReturnValueName("ToastButton")]
#endif
        public ToastButton AddArgument(string key, string value)
        {
            return AddArgumentHelper(key, value);
        }

        /// <summary>
        /// Adds a key/value to the activation arguments that will be returned when the toast notification or its buttons are clicked.
        /// </summary>
        /// <param name="key">The key for this value.</param>
        /// <param name="value">The value itself.</param>
        /// <returns>The current instance of <see cref="ToastButton"/></returns>
#if WINRT
        [return: System.Runtime.InteropServices.WindowsRuntime.ReturnValueName("ToastButton")]
#endif
        public ToastButton AddArgument(string key, int value)
        {
            return AddArgumentHelper(key, value.ToString(CultureInfo.InvariantCulture));
        }

        /// <summary>
        /// Adds a key/value to the activation arguments that will be returned when the toast notification or its buttons are clicked.
        /// </summary>
        /// <param name="key">The key for this value.</param>
        /// <param name="value">The value itself.</param>
        /// <returns>The current instance of <see cref="ToastButton"/></returns>
#if WINRT
        [return: System.Runtime.InteropServices.WindowsRuntime.ReturnValueName("ToastButton")]
#endif
        public ToastButton AddArgument(string key, double value)
        {
            return AddArgumentHelper(key, value.ToString(CultureInfo.InvariantCulture));
        }

        /// <summary>
        /// Adds a key/value to the activation arguments that will be returned when the toast notification or its buttons are clicked.
        /// </summary>
        /// <param name="key">The key for this value.</param>
        /// <param name="value">The value itself.</param>
        /// <returns>The current instance of <see cref="ToastButton"/></returns>
#if WINRT
        [return: System.Runtime.InteropServices.WindowsRuntime.ReturnValueName("ToastButton")]
#endif
        public ToastButton AddArgument(string key, float value)
        {
            return AddArgumentHelper(key, value.ToString(CultureInfo.InvariantCulture));
        }

        /// <summary>
        /// Adds a key/value to the activation arguments that will be returned when the toast notification or its buttons are clicked.
        /// </summary>
        /// <param name="key">The key for this value.</param>
        /// <param name="value">The value itself.</param>
        /// <returns>The current instance of <see cref="ToastButton"/></returns>
#if WINRT
        [return: System.Runtime.InteropServices.WindowsRuntime.ReturnValueName("ToastButton")]
#endif
        public ToastButton AddArgument(string key, bool value)
        {
            return AddArgumentHelper(key, value ? "1" : "0"); // Encode as 1 or 0 to save string space
        }

#if !WINRT
        /// <summary>
        /// Adds a key/value to the activation arguments that will be returned when the toast notification or its buttons are clicked.
        /// </summary>
        /// <param name="key">The key for this value.</param>
        /// <param name="value">The value itself. Note that the enums are stored using their numeric value, so be aware that changing your enum number values might break existing activation of toasts currently in Action Center.</param>
        /// <returns>The current instance of <see cref="ToastButton"/></returns>
        public ToastButton AddArgument(string key, Enum value)
        {
            return AddArgumentHelper(key, ((int)(object)value).ToString());
        }
#endif

        private ToastButton AddArgumentHelper(string key, string value)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            if (UsingCustomArguments)
            {
                throw new InvalidOperationException("You cannot use the AddArgument methods if you've set the Arguments property. Use the default ToastButton constructor instead.");
            }

            if (ActivationType == ToastActivationType.Protocol)
            {
                throw new InvalidOperationException("You cannot use the AddArgument methods when using protocol activation.");
            }

            if (ShouldDissmiss) //|| _usingSnoozeActivation)
            {
                throw new InvalidOperationException("You cannot use the AddArgument methods when using dismiss or snooze activation.");
            }

            bool alreadyExists = _arguments.ContainsKey(key);

            _arguments[key] = value;

            Arguments = alreadyExists ? SerializeArgumentsHelper(_arguments) : AddArgumentHelper(Arguments, key, value);

            return this;
        }

        private string SerializeArgumentsHelper(IDictionary<string, string> arguments)
        {
            var args = new ToastArguments();

            foreach (var a in arguments)
            {
                args.Add(a.Key, a.Value);
            }

            return args.ToString();
        }

        private string AddArgumentHelper(string existing, string key, string value)
        {
            string pair = ToastArguments.EncodePair(key, value);

            if (string.IsNullOrEmpty(existing))
            {
                return pair;
            }
            else
            {
                return existing + ToastArguments.Separator + pair;
            }
        }

        /// <summary>
        /// Configures the button to use background activation when the button is clicked.
        /// </summary>
        /// <returns>The current instance of ToastButton</returns>
        public ToastButton SetBackgroundActivation()
        {
            ActivationType = ToastActivationType.Background;
            return this;
        }

        /// <summary>
        /// Sets the text to display on the button.
        /// </summary>
        /// <param name="content">The text to display on the button.</param>
        /// <returns>The current instance of ToastButton</returns>
        public ToastButton SetContent(string content)
        {
            Content = content;
            return this;
        }

        /// <summary>
        /// Configures the button to use system dismiss activation when the button is clicked (the toast will simply dismiss rather than activating).
        /// </summary>
        /// <returns>The current instance of ToastButton</returns>
        public ToastButton SetDismissActivation()
        {
            ShouldDissmiss = true;
            return this;
        }

        /// <summary>
        /// Sets an identifier used in telemetry to identify your category of action. This should be something like "Delete", "Reply", or "Archive". In the upcoming toast telemetry dashboard in Dev Center, you will be able to view how frequently your actions are being clicked.
        /// </summary>
        /// <param name="actionId">An identifier used in telemetry to identify your category of action.</param>
        /// <returns>The current instance of ToastButton</returns>
        public ToastButton SetHintActionId(string actionId)
        {
            HintActionId = actionId;
            return this;
        }

        /// <summary>
        /// Sets an optional image icon for the button to display (required for buttons adjacent to inputs like quick reply).
        /// </summary>
        /// <param name="imageUri">An optional image icon for the button to display.</param>
        /// <returns>The current instance of ToastButton</returns>
        public ToastButton SetImageUri(Uri imageUri)
        {
            ImageUri = imageUri.ToString();
            return this;
        }

        /// <summary>
        /// Configures the button to launch the specified url when the button is clicked.
        /// </summary>
        /// <param name="protocol">The protocol to launch.</param>
        /// <returns>The current instance of ToastButton</returns>
        public ToastButton SetProtocolActivation(Uri protocol)
        {
            ActivationType = ToastActivationType.Protocol;
            Protocol = protocol;
            return this;
        }

        /// <summary>
        /// Configures the button to use system snooze activation when the button is clicked, using the default system snooze time.
        /// </summary>
        /// <returns>The current instance of ToastButton</returns>
        public ToastButton SetSnoozeActivation()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Configures the button to use system snooze activation when the button is clicked, with a snooze time defined by the specified selection box.
        /// </summary>
        /// <param name="selectionBoxId">The ID of an existing ToastSelectionBox which allows the user to pick a custom snooze time.</param>
        /// <returns>The current instance of ToastButton</returns>
        public ToastButton SetSnoozeActivation(string selectionBoxId)
        {
            SnoozeSelectionBoxId = selectionBoxId;
            return SetSnoozeActivation();
        }

        /// <summary>
        /// Sets the ID of an existing ToastTextBox in order to have this button display to the right of the input, achieving a quick reply scenario.
        /// </summary>
        /// <param name="textBoxId">The ID of an existing ToastTextBox.</param>
        /// <returns>The current instance of ToastButton</returns>
        public ToastButton SetTextBoxId(string textBoxId)
        {
            TextBoxId = textBoxId;
            return this;
        }


        internal bool ShouldDissmiss { get; private set; }
        internal Uri Protocol { get; private set; }
        internal string SnoozeSelectionBoxId { get; private set; }
        internal bool UsingCustomArguments { get; private set; }

        internal bool CanAddArguments()
        {
            // To Do: Add (!_usingSnoozeActivation) when implemented.
            return ActivationType != ToastActivationType.Protocol && !UsingCustomArguments && !ShouldDissmiss;
        }

        private readonly Dictionary<string, string> _arguments = new Dictionary<string, string>();
    }
}
