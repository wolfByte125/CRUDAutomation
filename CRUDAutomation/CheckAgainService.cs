using Microsoft.EntityFrameworkCore;
using AutoMapper;
using CRUDAutomation.Context
using CRUDAutomation.Models;
using CRUDAutomation.DTOs.CheckAgainDTOs;

namespace CRUDAutomation.Services.CheckAgainServices
{
	public class CheckAgainService : ICheckAgainService
	{
		private readonly DataContext _context;
		private readonly IMapper _mapper;
		public CheckAgainService(DataContext context, IMapper mapper)
		{
			_context = context;
			_mapper = mapper;
		}

		// GET ALL
		public async Task<List<CheckAgain>> GetCheckAgains()
		{
			var checkAgains = await _context.CheckAgains
				.OrderByDescending(x => x.Id)
				.ToListAsync();

			return checkAgains;
		}

		// GET SINGLE
		public async Task<CheckAgain> GetCheckAgain(int id)
		{
			var checkAgain = await _context.CheckAgains
				.Where(x => x.Id == id)
				.FirstOrDefaultAsync();

			if(checkAgain == null) throw new KeyNotFoundException("Check Again Not Found.");

			return checkAgain;
		}

		// CREATE || REQUEST
		public async Task<CheckAgain> RequestCheckAgain(RequestCheckAgainDTO checkAgainDTO)
		{
			var checkAgain = _mapper.Map<CheckAgain>(checkAgainDTO);

			_context.CheckAgains.Add(checkAgain);
			await _context.SaveChangesAsync();

			return checkAgain;
		}


	}
}
