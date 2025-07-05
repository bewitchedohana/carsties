using AuctionService.Data;
using AuctionService.DTOs;
using AuctionService.Entities;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuctionService.Controllers;

[ApiController]
[Route("api/auctions")]
public class AuctionsController : ControllerBase
{
    private readonly AuctionDbContext _context;
    private readonly IMapper _mapper;

    public AuctionsController(AuctionDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<ActionResult<List<AuctionDTO>>> GetAllAuctions([FromQuery] string? date)
    {
        var query = _context.Auctions
            .Include(a => a.Item)
            .OrderBy(a => a.Item.Make)
            .AsQueryable();

        if (!string.IsNullOrEmpty(date))
        {
            query = query.Where(a => a.UpdatedAt.CompareTo(DateTime.Parse(date).ToUniversalTime()) > 0);
        }

        return await query.ProjectTo<AuctionDTO>(_mapper.ConfigurationProvider)
            .ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<AuctionDTO>> GetAuctionById(Guid id)
    {
        var auction = await _context.Auctions
            .Include(a => a.Item)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (auction == null)
        {
            return NotFound();
        }

        return _mapper.Map<AuctionDTO>(auction);
    }

    [HttpPost]
    public async Task<ActionResult<AuctionDTO>> CreateAuction(CreateAuctionDTO auctionDTO)
    {
        var auction = _mapper.Map<Auction>(auctionDTO);
        // TODO: Add current user as seller
        auction.Seller = "test"; // Replace with actual user retrieval logic

        _context.Auctions.Add(auction);

        var result = await _context.SaveChangesAsync() > 0;
        if (!result)
        {
            return BadRequest("Could not save changes to the database.");
        }

        var auctionDto = _mapper.Map<AuctionDTO>(auction);
        return CreatedAtAction(nameof(GetAuctionById), new { id = auction.Id }, auctionDto);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateAuction(Guid id, UpdateAuctionDTO auctionDTO)
    {
        var auction = await _context.Auctions
            .Include(x => x.Item)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (auction == null)
        {
            return NotFound();
        }

        // TODO: Check if seller == username
        auction.Item.Make = auctionDTO.Make ?? auction.Item.Make;
        auction.Item.Model = auctionDTO.Model ?? auction.Item.Model;
        auction.Item.Color = auctionDTO.Color ?? auction.Item.Color;
        auction.Item.Mileage = auctionDTO.Mileage ?? auction.Item.Mileage;
        auction.Item.Year = auctionDTO.Year ?? auction.Item.Year;

        var result = await _context.SaveChangesAsync() > 0;
        if (result)
        {
            return NoContent();
        }

        return BadRequest("Could not update the auction.");
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAuction(Guid id)
    {
        var auction = await _context.Auctions
            .FindAsync(id);

        if (auction == null)
        {
            return NotFound();
        }

        // TODO: Check if seller == username
        _context.Auctions.Remove(auction);
        var result = await _context.SaveChangesAsync() > 0;
        if (!result)
        {
            return BadRequest("Could not delete the auction.");
        }

        return NoContent();
    }
}