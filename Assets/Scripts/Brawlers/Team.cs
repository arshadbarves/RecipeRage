using System.Collections.Generic;

namespace Brawlers
{
    public class Team
    {

        public Team(string teamID, int maxSize)
        {
            TeamID = teamID;
            MaxSize = maxSize;
            Members = new List<BaseController>();
        }
        public string TeamID { get; private set; }
        public int MaxSize { get; }
        public List<BaseController> Members { get; }

        public bool AddMember(BaseController member)
        {
            if (Members.Count >= MaxSize)
            {
                return false;
            }

            Members.Add(member);
            return true;
        }

        public bool RemoveMember(BaseController member)
        {
            return Members.Remove(member);
        }
    }
}