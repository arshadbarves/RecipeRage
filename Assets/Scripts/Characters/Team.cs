using System.Collections.Generic;

namespace Characters
{
    public class Team
    {
        public string TeamID { get; private set; }
        public int MaxSize { get; private set; }
        public List<BaseController> Members { get; private set; }
        
        public Team(string teamID, int maxSize)
        {
            TeamID = teamID;
            MaxSize = maxSize;
            Members = new List<BaseController>();
        }
        
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