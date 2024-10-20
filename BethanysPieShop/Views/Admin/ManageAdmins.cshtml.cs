using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

public class ManageAdminViewModel
{
    public List<IdentityUser> Users { get; set; }
    public List<string> Admins { get; set; }  // List of current admins (for display purposes)
}
