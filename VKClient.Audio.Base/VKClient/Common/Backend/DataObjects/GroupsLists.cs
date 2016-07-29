using System.Collections.Generic;

namespace VKClient.Common.Backend.DataObjects
{
  public class GroupsLists
  {
    public List<Group> Communities { get; set; }

    public int CommunitiesCount { get; set; }

    public List<Group> Events { get; set; }

    public int EventsCount { get; set; }

    public List<Group> AdminGroups { get; set; }

    public int AdminGroupsCount { get; set; }

    public CommunityInvitations Invitations { get; set; }
  }
}
