using BethanysPieShop.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BethanysPieShop.Areas.Identity
{
    public class SeedData
    {
        public static async Task Initialize(IServiceProvider serviceProvider, UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager, AppDbContext context)
        {
            var roleNames = new[] { "Admin", "User" };

            // Ensure roles exist
            foreach (var roleName in roleNames)
            {
                var roleExist = await roleManager.RoleExistsAsync(roleName);
                if (!roleExist)
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            var adminUser = await userManager.FindByEmailAsync("admin@admin.com");
            if (adminUser == null)
            {
                adminUser = new IdentityUser
                {
                    UserName = "admin@admin.com",
                    Email = "admin@admin.com"
                };

                // Create the admin user with the password
                var adminPassword = "Admin123!"; // Set a default admin password
                var result = await userManager.CreateAsync(adminUser, adminPassword);

                if (result.Succeeded)
                {
                    // Assign Admin role to the admin user
                    if (!await userManager.IsInRoleAsync(adminUser, "Admin"))
                    {
                        await userManager.AddToRoleAsync(adminUser, "Admin");
                    }
                }
            }
            else if (!await userManager.IsInRoleAsync(adminUser, "Admin"))
            {
                // Add the user to the Admin role if they are not already in it
                await userManager.AddToRoleAsync(adminUser, "Admin");
            }

            // Dynamic User Seeding Logic (fetch users from the database)
            var dynamicUsers = await GetDynamicUsersFromDatabase(context); // Fetch dynamic user data from the database

            foreach (var userData in dynamicUsers)
            {
                var user = await userManager.FindByEmailAsync(userData.Email);
                if (user == null)
                {
                    user = new IdentityUser
                    {
                        UserName = userData.Email,
                        Email = userData.Email
                    };
                    var result = await userManager.CreateAsync(user, userData.Password);  // Use dynamic password from the database
                    if (result.Succeeded)
                    {
                        // Assign role dynamically from the database
                        var roleExist = await roleManager.RoleExistsAsync(userData.Role);
                        if (roleExist)
                        {
                            await userManager.AddToRoleAsync(user, userData.Role);  // Assign role dynamically
                        }
                    }
                }
                else if (!await userManager.IsInRoleAsync(user, userData.Role))
                {
                    var roleExist = await roleManager.RoleExistsAsync(userData.Role);
                    if (roleExist)
                    {
                        await userManager.AddToRoleAsync(user, userData.Role);  // Assign role dynamically
                    }
                }
            }
        }

        // Fetch users from the database
        private static async Task<List<DynamicUser>> GetDynamicUsersFromDatabase(AppDbContext context)
        {
            // Fetch user data from your Users table (replace 'User' with your actual entity name)
            var userList = await context.Users
                .Select(u => new DynamicUser
                {
                    Email = u.Email, // Map your entity properties here
                    Password = u.PasswordHash,  // You may generate passwords dynamically or use a default one
                    Role = "User" // Assign a default role or fetch from a related table if needed
                })
                .ToListAsync();

            return userList;
        }
    }

    // Define a class to hold the dynamic user information (similar to the one before)
    public class DynamicUser
    {
        public string Email { get; set; }
        public string Password { get; set; } // You might generate passwords dynamically for security reasons
        public string Role { get; set; }  // Store the role assigned to the user (this could come from your User entity)
    }
}

