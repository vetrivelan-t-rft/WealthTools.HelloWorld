using System;
using System.Collections.Generic;
using System.Text;

namespace WealthTools.Library.Contacts.Models
{
  
    public class Team
    {
        public enum TeamRole
        {
            Primary = 1,
            Secondary = 2
        }
        public enum TeamAssignmentType
        {
            UnKnown = 0,
            SystemAssignedAcct = 1,
            SystemAssigned = 2,
            UserCreated = 3
        }
        public enum TeamHierarchyType
        {
            UnKnown = 0,
            sub = 1,
            Branch = 2,
            Rep = 3,
            NfsBranch = 5,
            NfsIndividual = 6
        }

        public string Broker_ID { get; set; }
         public string TeamId { get; set; }
         public string RepCode { get; set; }
       public string Name { get; set; }
        public string City { get; set; }
         public string State { get; set; }
        public string Email { get; set; }
        public TeamRole Role { get; set; }
        public TeamAssignmentType AssignType { get; set; }
        private int AssignTypeAsInt { get; set; }
        public TeamHierarchyType HierarchyType { get; set; }
         private int HierArchyTypeAsInt { get; set; }
    }
}

