using System;

namespace GymMemberManager
{
    public class MemberModel
    {
        public string MemberId { get; set; } = "";
        public string FullName { get; set; } = "";
        public string Phone { get; set; } = "";
        public string Package { get; set; } = "";
        public DateTime StartDate { get; set; }
        public decimal Fee { get; set; }
    }
}
