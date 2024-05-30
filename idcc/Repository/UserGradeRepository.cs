using idcc.Context;
using idcc.Models;
using idcc.Repository.Interfaces;

namespace idcc.Repository;

public class UserGradeRepository : IUserGradeRepository
{
    private readonly IdccContext _context;

    public UserGradeRepository(IdccContext context)
    {
        _context = context;
    }
    
    public UserGrade GetActualUserDataAsync(int userId)
    {
        var userGrade = _context.UserGrades.Where(_ => _.IsFinished == false && _.User.Id == userId).OrderBy(o => Guid.NewGuid()).First();
        return userGrade;
    }
    
    
}