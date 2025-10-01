using Microsoft.AspNetCore.Http.HttpResults;
using remotestorage_api.Models;

namespace remotestorage_api.Services;

public static class ImageService
{
    static List<Image> Images;
    static int nextId = 3;
    static ImageService()
    {
        Images = new List<Image>
        {
            new Image {Id = 1, Description = "The first image ever!", Content = ""},
            new Image {Id = 2, Description = "The second image?", Content = ""}
        };
    }

    public static List<Image> GetAll()
    {
        return Images;
    }

    public static Image? Get(int ImageId)
    {
        Image found = Images.FirstOrDefault(img => img.Id == ImageId);

        if (found != null)
        {
            Console.WriteLine($"Found: {found.Id}");
        }
        else
        {
            Console.WriteLine("Not found.");
        }

        return found;
    }

    public static void Add(Image image)
    {
        image.Id = nextId++;
        Images.Add(image);
    }

    public static void Delete(int id)
    {
        var result = Get(id);
        if (result is not null)
        {
            Images.Remove(result);
            return;
        }
        Console.WriteLine($"Image ({id}) removed!");
    }

    public static void Update(Image image)
    {

    }

}