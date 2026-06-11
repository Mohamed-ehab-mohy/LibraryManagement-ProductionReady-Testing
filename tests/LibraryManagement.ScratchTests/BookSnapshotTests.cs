using LibraryManagement.Services.DTOs;

namespace LibraryManagement.ScratchTests;

// Snapshot tests are useful when you want to detect unintended changes to output shape (e.g., API responses, serialization).
// They become a maintenance burden when the output changes frequently (e.g., dates, generated IDs, volatile data).
// Use sparingly for stable contracts; avoid for dynamic or frequently evolving data.
public class BookSnapshotTests
{
    [Fact]
    public Task BookDto_should_match_snapshot()
    {
        var book = new BookDto
        {
            Id = 1,
            Title = "Clean Code",
            Author = "Robert C. Martin",
            ISBN = "9780132350884",
            TotalCopies = 5,
            AvailableCopies = 3
        };

        return Verifier.Verify(book);
    }
}
