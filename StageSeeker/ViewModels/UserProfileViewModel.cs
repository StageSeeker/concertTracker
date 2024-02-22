using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StageSeeker.ViewModels
{
    public class UserProfileViewModel
    {
        public string EmailAddress { get; set; } =  null!;

        public string Name { get; set; } = null!;

        public string ProfileImage { get; set; } = null!;
    }
}
