using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace JU.Examination.ViewModel
{
    /// <summary>
    /// Message status.
    /// </summary>
    public enum MessageStatusViewModel
    {
        NOMESSAGE,
        ERROR,
        INFO,
        SUCCESS,
        WARNING
    }

    /// <summary>
    /// Message in the ViewModels (to show the user).
    /// </summary>
    public class MessageViewModel
    {
        /// <summary>
        /// The status.
        /// </summary>
        public MessageStatusViewModel Status { get; set; }

        /// <summary>
        /// The title.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Message text.
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MessageViewModel"/> class.
        /// </summary>
        public MessageViewModel()
        {
            Status = MessageStatusViewModel.NOMESSAGE;
            Title = "";
            Text = "";
        }

    }
}