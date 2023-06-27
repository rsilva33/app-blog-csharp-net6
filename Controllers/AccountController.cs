using Blog.Extensions;
using Blog.Models;
using Blog.ViewModels;
using Blog.ViewModels.Accounts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SecureIdentity.Password;
using System.Text.RegularExpressions;

namespace Blog.Controllers;

[ApiController]
public class AccountController : ControllerBase
{
    [HttpPost("v1/accounts/")]
    public async Task<IActionResult> Post
    (
        [FromBody] RegisterViewModel model,
        [FromServices] EmailService emailService,
        [FromServices] BlogDataContext context
    )
    {
        if (!ModelState.IsValid)
            return BadRequest(new ResultViewModel<string>(ModelState.GetErrors()));

        var user = new User
        {
            Name = model.Email,
            Email = model.Email,
            Slug = model.Email.Replace("@", "-").Replace(".", "-"),
            Bio = "test"

        };

        var password = PasswordGenerator.Generate(25);
        user.PasswordHash = PasswordHasher.Hash(password);

        try
        {
            await context.Users.AddAsync(user);
            await context.SaveChangesAsync();

            emailService.Send(
                              user.Name,
                              user.Email,
                              "Welcome to blog!",
                              $"This password is <strong>{password}</string>");

            return Ok(new ResultViewModel<dynamic>(new
            {
                user = user.Email,
                password
            }));
        }
        catch (DbUpdateException)
        {
            return StatusCode(400, new ResultViewModel<string>("05X99 - This e-mail is already registered."));
        }
        catch
        {
            return StatusCode(500, new ResultViewModel<string>("05X04 - Internar server error."));
        }
    }

    [HttpPost("v1/accounts/login")]
    public async Task<IActionResult> Login
    (
        [FromBody] LoginViewModel model,
        [FromServices] BlogDataContext context,
        [FromServices] TokenService tokenService
    )
    {
        if (!ModelState.IsValid)
            return BadRequest(new ResultViewModel<string>(ModelState.GetErrors()));

        var user = await context.Users
                        .AsNoTracking()
                            .Include(x => x.Roles)
                                .FirstOrDefaultAsync(x => x.Email == model.Email);

        if (user == null)
            return StatusCode(401, new ResultViewModel<string>("User or password invalid."));

        if (!PasswordHasher.Verify(user.PasswordHash, model.Password))
            return StatusCode(401, new ResultViewModel<string>("User or password invalid."));

        //var token = tokenService.GenerateToken(null);
        try
        {
            var token = tokenService.GenerateToken(user);
            return Ok(new ResultViewModel<string>(token, null));
        }
        catch
        {
            return StatusCode(500, new ResultViewModel<string>("05X13 - Internal server error."));
        }
    }

    [Authorize]
    [HttpPost("v1/accounts/upload-image")]
    public async Task<IActionResult> UploadImage
    (
        [FromBody] UploadImageViewModel model,
        [FromServices] BlogDataContext context
    )
    {
        var fileName = $"{Guid.NewGuid().ToString()}.jpg";
        var data = new Regex(@"^data:image\/[a-z]+;base64,")
                                    .Replace(model.Base64Image, "");
        var bytes = Convert.FromBase64String(data);

        try
        {
            await System.IO.File.WriteAllBytesAsync($"wwwroot/images/{fileName}", bytes);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ResultViewModel<string>("05X04 - Internal server error."));
        }
        
        var user = await context.Users.FirstOrDefaultAsync(x => x.Name == User.Identity.Name);

        if (user == null)
            return NotFound(new ResultViewModel<User>("User not found."));

        user.Image = $"https://localhost:0000/images/{fileName}";

        try
        {
            context.Users.Update(user);
            await context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ResultViewModel<string>("05X04 - Internal server error."));
        }

        return Ok(new ResultViewModel<string>("Successfully changed image.", null));
    }

    //[Authorize(Roles = "user")]
    //[HttpGet("v1/user")]
    //public IActionResult GetUser() => Ok(User.Identity.Name);

    //[Authorize(Roles = "author")]
    //[HttpGet("v1/author")]
    //public IActionResult GetAuthor() => Ok(User.Identity.Name);

    //[Authorize(Roles = "admin")]
    //[HttpGet("v1/admin")]
    //public IActionResult GetAdmin() => Ok(User.Identity.Name);
}
