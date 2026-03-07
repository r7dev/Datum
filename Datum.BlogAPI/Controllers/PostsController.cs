using Datum.Domain.DTOs;
using Datum.Domain.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Datum.BlogAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PostsController(IPostService postService) : ControllerBase
{
	private readonly IPostService _postService = postService;

	private int GetCurrentUserId()
	{
		var claim = User.FindFirstValue(ClaimTypes.NameIdentifier)
			?? throw new UnauthorizedAccessException("Usuário não autenticado.");
		return int.Parse(claim);
	}

	/// <summary>Lista todas as postagens (público)</summary>
	[HttpGet]
	[ProducesResponseType(typeof(IEnumerable<PostResponse>), StatusCodes.Status200OK)]
	public async Task<IActionResult> GetAll()
	{
		var posts = await _postService.GetAllPostsAsync();
		return Ok(posts);
	}

	/// <summary>Retorna uma postagem pelo ID (público)</summary>
	[HttpGet("{id:int}")]
	[ProducesResponseType(typeof(PostResponse), StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	public async Task<IActionResult> GetById(int id)
	{
		var post = await _postService.GetPostByIdAsync(id);
		return post is null ? NotFound(new { message = "Post não encontrado." }) : Ok(post);
	}

	/// <summary>Cria uma nova postagem (requer autenticação)</summary>
	[HttpPost]
	[Authorize]
	[ProducesResponseType(typeof(PostResponse), StatusCodes.Status201Created)]
	[ProducesResponseType(StatusCodes.Status401Unauthorized)]
	public async Task<IActionResult> Create([FromBody] CreatePostRequest request)
	{
		var userId = GetCurrentUserId();
		var post   = await _postService.CreatePostAsync(request, userId);
		return CreatedAtAction(nameof(GetById), new { id = post.Id }, post);
	}

	/// <summary>Edita uma postagem (somente o autor)</summary>
	[HttpPut("{id:int}")]
	[Authorize]
	[ProducesResponseType(typeof(PostResponse), StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status403Forbidden)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	public async Task<IActionResult> Update(int id, [FromBody] UpdatePostRequest request)
	{
		try
		{
			var userId = GetCurrentUserId();
			var post   = await _postService.UpdatePostAsync(id, request, userId);
			return Ok(post);
		}
		catch (KeyNotFoundException ex)   { return NotFound(new { message = ex.Message }); }
		catch (UnauthorizedAccessException) { return Forbid(); }
	}

	/// <summary>Exclui uma postagem (somente o autor)</summary>
	[HttpDelete("{id:int}")]
	[Authorize]
	[ProducesResponseType(StatusCodes.Status204NoContent)]
	[ProducesResponseType(StatusCodes.Status403Forbidden)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	public async Task<IActionResult> Delete(int id)
	{
		try
		{
			var userId = GetCurrentUserId();
			await _postService.DeletePostAsync(id, userId);
			return NoContent();
		}
		catch (KeyNotFoundException ex)   { return NotFound(new { message = ex.Message }); }
		catch (UnauthorizedAccessException) { return Forbid(); }
	}
}
