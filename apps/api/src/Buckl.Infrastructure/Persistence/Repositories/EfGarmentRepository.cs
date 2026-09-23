using Buckl.Domain.Garments;
using Buckl.Domain.Users;
using Buckl.Infrastructure.Persistence.Mapping;
using Buckl.Infrastructure.Persistence.Records;
using Microsoft.EntityFrameworkCore;

namespace Buckl.Infrastructure.Persistence.Repositories;

/// <summary>Garments through EF Core. Row-Level Security decides which rows exist for the
/// current user; the owner filter in <see cref="ListByOwnerAsync"/> is the second layer
/// (ADR-0007). Filters and order match the web app's in-memory repository.</summary>
public sealed class EfGarmentRepository : IGarmentRepository
{
    private readonly BucklDbContext _context;

    public EfGarmentRepository(BucklDbContext context)
    {
        _context = context;
    }

    public async Task<Garment?> GetByIdAsync(GarmentId id, CancellationToken cancellationToken = default)
    {
        // Tracked on purpose: a later UpdateAsync in the same request copies onto this record.
        var record = await _context.Garments
            .SingleOrDefaultAsync(garment => garment.Id == id.Value, cancellationToken);

        return record is null ? null : GarmentMapper.ToDomain(record);
    }

    public async Task<IReadOnlyList<Garment>> ListByOwnerAsync(
        UserId ownerId,
        WardrobeFilter filter,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(filter);

        var records = await Filtered(ownerId, filter)
            .OrderByDescending(garment => garment.CreatedAt)
            .ThenBy(garment => garment.Id)
            .ToListAsync(cancellationToken);

        return records.ConvertAll(GarmentMapper.ToDomain);
    }

    public Task AddAsync(Garment garment, CancellationToken cancellationToken = default)
    {
        _context.Garments.Add(GarmentMapper.ToRecord(garment));

        return Task.CompletedTask;
    }

    public async Task UpdateAsync(Garment garment, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(garment);

        var record = await _context.Garments.FindAsync([garment.Id.Value], cancellationToken)
            ?? throw new InvalidOperationException(
                $"Garment {garment.Id} is not stored or not visible to the current user.");

        GarmentMapper.Apply(garment, record);
    }

    private IQueryable<GarmentRecord> Filtered(UserId ownerId, WardrobeFilter filter)
    {
        var owner = ownerId.Value;
        var status = EnumText.ToText(filter.Status);
        var query = _context.Garments
            .AsNoTracking()
            .Where(garment => garment.UserId == owner && garment.Status == status);

        if (filter.Category is { } category)
        {
            var text = EnumText.ToText(category);
            query = query.Where(garment => garment.Category == text);
        }

        if (filter.Color is { } color)
        {
            var text = EnumText.ToText(color);
            query = query.Where(garment => garment.Color == text);
        }

        if (filter.Size is { } size)
        {
            var pattern = LikePattern.Escape(size.Label);
            query = query.Where(garment =>
                garment.Size != null
                && EF.Functions.ILike(garment.Size, pattern, LikePattern.EscapeCharacter));
        }

        if (filter.SearchText is { } searchText)
        {
            var pattern = LikePattern.Contains(searchText);
            query = query.Where(garment =>
                EF.Functions.ILike(
                    garment.Category
                        + " " + garment.Color
                        + (garment.Notes != null ? " " + garment.Notes : "")
                        + (garment.Size != null ? " " + garment.Size : "")
                        + (garment.Product != null ? " " + garment.Product.Name : "")
                        + (garment.Product != null && garment.Product.Brand != null
                            ? " " + garment.Product.Brand
                            : ""),
                    pattern,
                    LikePattern.EscapeCharacter));
        }

        return query;
    }
}
