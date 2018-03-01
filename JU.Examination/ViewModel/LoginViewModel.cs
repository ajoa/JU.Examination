using System;
using System.ComponentModel.DataAnnotations;

namespace JU.Examination.ViewModel
{
    public enum UserAccessRole
    {
        NOT_SET = 0,
        ADMIN = 1,
        STAFF = 2,
        STUDENT = 3
    }

    /// <summary>
    /// 
    /// </summary>
    public class LoginViewModel
    {
        /// <summary>
        /// Gets or sets the name of the user.
        /// </summary>
        [Required]
        public string UserName { get; set; }

        /// <summary>
        /// The password.
        /// </summary>
        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether remember the sign in.
        /// </summary>
        public bool RememberMe { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [user name valid].
        /// </summary>
        public bool UserNameValid { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [password valid].
        /// </summary>
        public bool PasswordValid { get; set; }

        public MessageViewModel Message { get; set; }

        /// <summary>
        /// The roll used in the application
        /// </summary>
        public UserAccessRole Roles { get; set; }

        /// <summary>
        /// Loading userAccessRole as string
        /// </summary>
        public string RolesNames
        {
            get
            {
                string rolesName = "";
                switch (this.Roles)
                {
                    case UserAccessRole.NOT_SET:
                        rolesName = "";
                        break;
                    case UserAccessRole.ADMIN:
                        rolesName = "Admin";
                        break;
                    case UserAccessRole.STUDENT:
                        rolesName = "Student";
                        break;
                    case UserAccessRole.STAFF:
                        rolesName = "Staff";
                        break;
                    default:
                        throw new ArgumentException("Access role not defined in RolesNames");
                }

                return rolesName;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LoginViewModel"/> class.
        /// </summary>
        public LoginViewModel()
        {
            UserName = "";
            Password = "";
            RememberMe = false;
            PasswordValid = true;
            UserNameValid = true;
            Message = new MessageViewModel();
            Roles = UserAccessRole.NOT_SET;
        }
    }
}