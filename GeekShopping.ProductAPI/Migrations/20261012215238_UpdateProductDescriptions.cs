using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GeekShopping.ProductAPI.Migrations
{
    /// <inheritdoc />
    public partial class UpdateProductDescriptions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "product",
                keyColumn: "id",
                keyValue: 3L,
                column: "description",
                value: "Black T-shirt with a 'No Internet' print in the style of a classic dinosaur park logo. For anyone who has stared at an offline browser tab waiting for the connection to come back.");

            migrationBuilder.UpdateData(
                table: "product",
                keyColumn: "id",
                keyValue: 4L,
                column: "description",
                value: "Full-size Darth Vader helmet from the Star Wars Black Series collection, made in three pieces that fit together like the one on screen. A display piece for collectors, and wearable too.");

            migrationBuilder.UpdateData(
                table: "product",
                keyColumn: "id",
                keyValue: 5L,
                column: "description",
                value: "Imperial Stormtrooper action figure from the Star Wars Black Series line by Hasbro, with detailed armor, several points of articulation and a blaster accessory.");

            migrationBuilder.UpdateData(
                table: "product",
                keyColumn: "id",
                keyValue: 6L,
                column: "description",
                value: "Black T-shirt with a '100% Gamer' print surrounded by controller, headset and console icons. A comfortable everyday shirt for long gaming sessions.");

            migrationBuilder.UpdateData(
                table: "product",
                keyColumn: "id",
                keyValue: 7L,
                column: "description",
                value: "Raglan T-shirt with the SpaceX logo across the chest, white body and dark sleeves. For fans of rockets, launches and landings.");

            migrationBuilder.UpdateData(
                table: "product",
                keyColumn: "id",
                keyValue: 8L,
                column: "description",
                value: "Women's T-shirt in a light sand color with a vintage 'Benefits of Drinking Coffee' illustration. For people whose day only starts after the first cup.");

            migrationBuilder.UpdateData(
                table: "product",
                keyColumn: "id",
                keyValue: 9L,
                column: "description",
                value: "Cobra Kai hooded sweatshirt with the dojo's snake logo on the front, front pocket and adjustable hood. Strike first, strike hard, no mercy.");

            migrationBuilder.UpdateData(
                table: "product",
                keyColumn: "id",
                keyValue: 10L,
                column: "description",
                value: "StarTalk, by Neil deGrasse Tyson: the book version of the science show, with answers about space travel, science fiction and the universe, explained with humor.");

            migrationBuilder.UpdateData(
                table: "product",
                keyColumn: "id",
                keyValue: 11L,
                column: "description",
                value: "Star Wars Mission Fleet set with a Han Solo figure and a Millennium Falcon he fits inside, plus projectile launchers. Sized for play, for ages 4 and up.");

            migrationBuilder.UpdateData(
                table: "product",
                keyColumn: "id",
                keyValue: 12L,
                column: "description",
                value: "T-shirt with an 'Occupy Mars' print over the red planet, inspired by SpaceX and the plan to take people there.");

            migrationBuilder.UpdateData(
                table: "product",
                keyColumn: "id",
                keyValue: 13L,
                column: "description",
                value: "Men's T-shirt for programmers with a GNU/Linux print. For people who live in the terminal and choose their distro with care.");

            migrationBuilder.UpdateData(
                table: "product",
                keyColumn: "id",
                keyValue: 14L,
                columns: new[] { "description", "image_url" },
                values: new object[] { "T-shirt showing Goku through his transformations in Dragon Ball, from his base form to Super Saiyan.", "https://github.com/leandrocgsi/erudio-microservices-dotnet6/blob/main/ShoppingImages/13_dragon_ball.jpg?raw=true" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "product",
                keyColumn: "id",
                keyValue: 3L,
                column: "description",
                value: "It is a long established fact that a reader will be distracted by the readable content of a page when looking at its layout.<br/>The point of using Lorem Ipsum is that it has a more-or-less normal distribution of letters, as opposed to using 'Content here, content here', making it look like readable English.<br/>Many desktop publishing packages and web page editors now use Lorem Ipsum as their default model text, and a search for 'lorem ipsum' will uncover many web sites still in their infancy.");

            migrationBuilder.UpdateData(
                table: "product",
                keyColumn: "id",
                keyValue: 4L,
                column: "description",
                value: "It is a long established fact that a reader will be distracted by the readable content of a page when looking at its layout.<br/>The point of using Lorem Ipsum is that it has a more-or-less normal distribution of letters, as opposed to using 'Content here, content here', making it look like readable English.<br/>Many desktop publishing packages and web page editors now use Lorem Ipsum as their default model text, and a search for 'lorem ipsum' will uncover many web sites still in their infancy.");

            migrationBuilder.UpdateData(
                table: "product",
                keyColumn: "id",
                keyValue: 5L,
                column: "description",
                value: "It is a long established fact that a reader will be distracted by the readable content of a page when looking at its layout.<br/>The point of using Lorem Ipsum is that it has a more-or-less normal distribution of letters, as opposed to using 'Content here, content here', making it look like readable English.<br/>Many desktop publishing packages and web page editors now use Lorem Ipsum as their default model text, and a search for 'lorem ipsum' will uncover many web sites still in their infancy.");

            migrationBuilder.UpdateData(
                table: "product",
                keyColumn: "id",
                keyValue: 6L,
                column: "description",
                value: "It is a long established fact that a reader will be distracted by the readable content of a page when looking at its layout.<br/>The point of using Lorem Ipsum is that it has a more-or-less normal distribution of letters, as opposed to using 'Content here, content here', making it look like readable English.<br/>Many desktop publishing packages and web page editors now use Lorem Ipsum as their default model text, and a search for 'lorem ipsum' will uncover many web sites still in their infancy.");

            migrationBuilder.UpdateData(
                table: "product",
                keyColumn: "id",
                keyValue: 7L,
                column: "description",
                value: "It is a long established fact that a reader will be distracted by the readable content of a page when looking at its layout.<br/>The point of using Lorem Ipsum is that it has a more-or-less normal distribution of letters, as opposed to using 'Content here, content here', making it look like readable English.<br/>Many desktop publishing packages and web page editors now use Lorem Ipsum as their default model text, and a search for 'lorem ipsum' will uncover many web sites still in their infancy.");

            migrationBuilder.UpdateData(
                table: "product",
                keyColumn: "id",
                keyValue: 8L,
                column: "description",
                value: "It is a long established fact that a reader will be distracted by the readable content of a page when looking at its layout.<br/>The point of using Lorem Ipsum is that it has a more-or-less normal distribution of letters, as opposed to using 'Content here, content here', making it look like readable English.<br/>Many desktop publishing packages and web page editors now use Lorem Ipsum as their default model text, and a search for 'lorem ipsum' will uncover many web sites still in their infancy.");

            migrationBuilder.UpdateData(
                table: "product",
                keyColumn: "id",
                keyValue: 9L,
                column: "description",
                value: "It is a long established fact that a reader will be distracted by the readable content of a page when looking at its layout.<br/>The point of using Lorem Ipsum is that it has a more-or-less normal distribution of letters, as opposed to using 'Content here, content here', making it look like readable English.<br/>Many desktop publishing packages and web page editors now use Lorem Ipsum as their default model text, and a search for 'lorem ipsum' will uncover many web sites still in their infancy.");

            migrationBuilder.UpdateData(
                table: "product",
                keyColumn: "id",
                keyValue: 10L,
                column: "description",
                value: "It is a long established fact that a reader will be distracted by the readable content of a page when looking at its layout.<br/>The point of using Lorem Ipsum is that it has a more-or-less normal distribution of letters, as opposed to using 'Content here, content here', making it look like readable English.<br/>Many desktop publishing packages and web page editors now use Lorem Ipsum as their default model text, and a search for 'lorem ipsum' will uncover many web sites still in their infancy.");

            migrationBuilder.UpdateData(
                table: "product",
                keyColumn: "id",
                keyValue: 11L,
                column: "description",
                value: "It is a long established fact that a reader will be distracted by the readable content of a page when looking at its layout.<br/>The point of using Lorem Ipsum is that it has a more-or-less normal distribution of letters, as opposed to using 'Content here, content here', making it look like readable English.<br/>Many desktop publishing packages and web page editors now use Lorem Ipsum as their default model text, and a search for 'lorem ipsum' will uncover many web sites still in their infancy.");

            migrationBuilder.UpdateData(
                table: "product",
                keyColumn: "id",
                keyValue: 12L,
                column: "description",
                value: "It is a long established fact that a reader will be distracted by the readable content of a page when looking at its layout.<br/>The point of using Lorem Ipsum is that it has a more-or-less normal distribution of letters, as opposed to using 'Content here, content here', making it look like readable English.<br/>Many desktop publishing packages and web page editors now use Lorem Ipsum as their default model text, and a search for 'lorem ipsum' will uncover many web sites still in their infancy.");

            migrationBuilder.UpdateData(
                table: "product",
                keyColumn: "id",
                keyValue: 13L,
                column: "description",
                value: "It is a long established fact that a reader will be distracted by the readable content of a page when looking at its layout.<br/>The point of using Lorem Ipsum is that it has a more-or-less normal distribution of letters, as opposed to using 'Content here, content here', making it look like readable English.<br/>Many desktop publishing packages and web page editors now use Lorem Ipsum as their default model text, and a search for 'lorem ipsum' will uncover many web sites still in their infancy.");

            migrationBuilder.UpdateData(
                table: "product",
                keyColumn: "id",
                keyValue: 14L,
                columns: new[] { "description", "image_url" },
                values: new object[] { "It is a long established fact that a reader will be distracted by the readable content of a page when looking at its layout.<br/>The point of using Lorem Ipsum is that it has a more-or-less normal distribution of letters, as opposed to using 'Content here, content here', making it look like readable English.<br/>Many desktop publishing packages and web page editors now use Lorem Ipsum as their default model text, and a search for 'lorem ipsum' will uncover many web sites still in their infancy.", "https://github.com/leandrocgsi/erudio-microservices-dotnet6/blob/main/ShoppingImages/13_dragon_ball.jpg" });
        }
    }
}
