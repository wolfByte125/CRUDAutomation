using CRUDAutomation.Models;
using CRUDAutomation.DTOs.CheckAgainDTOs;

namespace CRUDAutomation.Services.CheckAgainServices
{
	public interface ICheckAgainService
	{
		// CRUD
		Task<List<CheckAgain>> GetCheckAgains();
		Task<CheckAgain> GetCheckAgain(int id);
		Task<CheckAgain> RequestCheckAgain(RequestCheckAgainDTO checkAgainDTO);
		Task<CheckAgain> UpdateCheckAgain(UpdateCheckAgainDTO checkAgainDTO);
		Task<CheckAgain> DeleteCheckAgain(int id);
		// STATUS UPDATE
		Task<CheckAgain> CheckCheckAgains(List<int> ids);
		Task<CheckAgain> ApproveCheckAgains(List<int> ids);
		Task<CheckAgain> DeclineCheckAgains(List<int> ids);
	}
}
