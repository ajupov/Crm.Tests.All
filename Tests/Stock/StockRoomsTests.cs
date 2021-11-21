using System;
using System.Linq;
using System.Threading.Tasks;
using Ajupov.Utils.All.DateTime;
using Crm.Tests.All.Extensions;
using Crm.Tests.All.Services.Creator;
using Crm.Tests.All.Services.DefaultRequestHeadersService;
using Crm.v1.Clients.Stock.Clients;
using Crm.v1.Clients.Stock.Models;
using Xunit;

namespace Crm.Tests.All.Tests.Stock
{
    public class StockRoomsTests
    {
        private readonly ICreate _create;
        private readonly IDefaultRequestHeadersService _defaultRequestHeadersService;
        private readonly IStockRoomsClient _stockRoomsClient;

        public StockRoomsTests(
            ICreate create,
            IDefaultRequestHeadersService defaultRequestHeadersService,
            IStockRoomsClient stockRoomsClient)
        {
            _create = create;
            _defaultRequestHeadersService = defaultRequestHeadersService;
            _stockRoomsClient = stockRoomsClient;
        }

        [Fact]
        public async Task WhenGet_ThenSuccess()
        {
            var headers = await _defaultRequestHeadersService.GetAsync();

            var roomId = (await _create.StockRoom.BuildAsync()).Id;

            var room = await _stockRoomsClient.GetAsync(roomId, headers);

            Assert.NotNull(room);
            Assert.Equal(roomId, room.Id);
        }

        [Fact]
        public async Task WhenGetList_ThenSuccess()
        {
            var headers = await _defaultRequestHeadersService.GetAsync();

            var roomIds = (
                    await Task.WhenAll(
                        _create.StockRoom
                            .WithName("Test".WithGuid())
                            .BuildAsync(),
                        _create.StockRoom
                            .WithName("Test".WithGuid())
                            .BuildAsync())
                )
                .Select(x => x.Id)
                .ToList();

            var rooms = await _stockRoomsClient.GetListAsync(roomIds, headers);

            Assert.NotEmpty(rooms);
            Assert.Equal(roomIds.Count, rooms.Count);
        }

        [Fact]
        public async Task WhenGetPagedList_ThenSuccess()
        {
            var headers = await _defaultRequestHeadersService.GetAsync();

            var name = "Test".WithGuid();
            await Task.WhenAll(
                _create.StockRoom
                    .WithName(name)
                    .BuildAsync());

            var request = new StockRoomGetPagedListRequest
            {
                Name = name
            };

            var response = await _stockRoomsClient.GetPagedListAsync(request, headers);

            var results = response.Rooms
                .Skip(1)
                .Zip(response.Rooms, (previous, current) => current.CreateDateTime >= previous.CreateDateTime);

            Assert.NotEmpty(response.Rooms);
            Assert.All(results, Assert.True);
        }

        [Fact]
        public async Task WhenCreate_ThenSuccess()
        {
            var headers = await _defaultRequestHeadersService.GetAsync();

            var room = new StockRoom
            {
                Id = Guid.NewGuid(),
                Name = "Test".WithGuid(),
                IsDeleted = false
            };

            var createdTypeId = await _stockRoomsClient.CreateAsync(room, headers);

            var createdType = await _stockRoomsClient.GetAsync(createdTypeId, headers);

            Assert.NotNull(createdType);
            Assert.Equal(createdTypeId, createdType.Id);
            Assert.Equal(room.Name, createdType.Name);
            Assert.Equal(room.IsDeleted, createdType.IsDeleted);
            Assert.True(createdType.CreateDateTime.IsMoreThanMinValue());
        }

        [Fact]
        public async Task WhenUpdate_ThenSuccess()
        {
            var headers = await _defaultRequestHeadersService.GetAsync();

            var room = await _create.StockRoom
                .WithName("Test".WithGuid())
                .BuildAsync();

            room.Name = "Test".WithGuid();
            room.IsDeleted = true;

            await _stockRoomsClient.UpdateAsync(room, headers);

            var updatedType = await _stockRoomsClient.GetAsync(room.Id, headers);

            Assert.Equal(room.Name, updatedType.Name);
            Assert.Equal(room.IsDeleted, updatedType.IsDeleted);
        }

        [Fact]
        public async Task WhenDelete_ThenSuccess()
        {
            var headers = await _defaultRequestHeadersService.GetAsync();

            var roomIds = (
                    await Task.WhenAll(
                        _create.StockRoom
                            .WithName("Test".WithGuid())
                            .BuildAsync(),
                        _create.StockRoom
                            .WithName("Test".WithGuid())
                            .BuildAsync())
                )
                .Select(x => x.Id)
                .ToList();

            await _stockRoomsClient.DeleteAsync(roomIds, headers);

            var rooms = await _stockRoomsClient.GetListAsync(roomIds, headers);

            Assert.All(rooms, x => Assert.True(x.IsDeleted));
        }

        [Fact]
        public async Task WhenRestore_ThenSuccess()
        {
            var headers = await _defaultRequestHeadersService.GetAsync();

            var roomIds = (
                    await Task.WhenAll(
                        _create.StockRoom
                            .WithName("Test".WithGuid())
                            .BuildAsync(),
                        _create.StockRoom
                            .WithName("Test".WithGuid())
                            .BuildAsync())
                )
                .Select(x => x.Id)
                .ToList();

            await _stockRoomsClient.RestoreAsync(roomIds, headers);

            var rooms = await _stockRoomsClient.GetListAsync(roomIds, headers);

            Assert.All(rooms, x => Assert.False(x.IsDeleted));
        }
    }
}
