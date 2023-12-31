using Microsoft.EntityFrameworkCore;
using PPI.Data.Context;
using PPI.Data.Repositories.Contracts;
using PPI.Models;
using PPI.Models.Dtos;
using PPI.Models.Enums;

namespace PPI.Data.Repositories;

public class QuestionRepository : RepositoryBase<Question>, IQuestionRepository
{
    public QuestionRepository(DataContext dataContext) : base(dataContext)
    {
    }

    public PaginationDto<DisplayQuestionDto> GetPagedQuestions(int pageIndex, int pageSize, string? filter, ESubject? subject, EDifficulty? difficulty)
    {
        var query = _dataContext.Questions
            .AsNoTracking()
            .Include(x => x.Answers)
            .OrderByDescending(x => x.CreatedAt)
            .Where(x => x.DeletedAt == null)
            .Select(x => new DisplayQuestionDto
            {
                Id = x.Id,
                Content = x.Content,
                Subject = x.Subject,
                Difficulty = x.Difficulty,
                Answers = x.Answers!.Select(y => new AnswerDto
                {
                    Content = y.Content,
                    Correct = y.Correct
                }).ToList()
            })
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter))
            query = query.Where(x => x.Content.ToLower().Contains(filter.ToLower()));

        if (subject != null)
            query = query.Where(x => x.Subject == subject);

        if (difficulty != null)
            query = query.Where(x => x.Difficulty == difficulty);

        var pagedQuery = new PaginationDto<DisplayQuestionDto>()
        {
            Data = query.Skip(pageSize * pageIndex).Take(pageSize).ToList(),
            Count = query.Count(),
        };

        return pagedQuery;
    }
}