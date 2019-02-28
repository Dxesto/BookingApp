﻿using System;
using System.Collections.Generic;
using Xunit;
using Moq;
using System.Threading.Tasks;
using BookingApp.Services;
using BookingApp.Data.Models;
using BookingApp.Controllers;
using Microsoft.AspNetCore.Mvc;
using BookingApp.DTOs;
using System.Linq;
using System.Net;

namespace BookingAppTests
{
    public class RuleControllerTesting
    {
        #region Initialize
        private Mock<IRuleService> mockServ;
        public RuleControllerTesting()
        {
            mockServ = new Mock<IRuleService>();
        }
        #endregion

        #region GetListRules
        [Fact]
        public async Task GetListRulesForAdmin()
        {
            //arrange
            mockServ.Setup(p => p.GetList()).ReturnsAsync(initRules());
            var mockContr = new Mock<RuleController>(mockServ.Object) { CallBase = true };
            mockContr.SetupGet(p => p.IsAdmin).Returns(true);

            //act
            var result = await mockContr.Object.Rules();

            //Assert
            var ruleOk = Assert.IsType<OkObjectResult>(result);
            var modelOk = Assert.IsAssignableFrom<List<RuleAdminDTO>>(ruleOk.Value);
            Assert.Equal(3, modelOk.Count);
            Assert.Equal("ComputerRule", modelOk.Find(p => p.Title == "ComputerRule").Title);
        }

        [Fact]
        public async Task GetListRulesForUser()
        {
            //arrange
            mockServ.Setup(p => p.GetActiveList()).ReturnsAsync(initRules().Where(p => p.IsActive == true));
            var mockContr = new Mock<RuleController>(mockServ.Object) { CallBase = true };
            mockContr.SetupGet(p => p.IsAdmin).Returns(false);

            //act
            var result = await mockContr.Object.Rules();

            //Assert
            var ruleOk = Assert.IsType<OkObjectResult>(result);
            var modelOk = Assert.IsAssignableFrom<List<RuleBasicDTO>>(ruleOk.Value);
            Assert.Equal(2, modelOk.Count);
            Assert.Equal("ComputerRule", modelOk.Find(p => p.Title == "ComputerRule").Title);
        }
        #endregion

        #region GetRule
        [Theory]
        [InlineData(2)]
        public async Task GetRuleForAdmin(int id)
        {
            //arrange
            mockServ.Setup(p => p.Get(id)).ReturnsAsync(initRules().Single(p => p.Id == id));
            var controller = new Mock<RuleController>(mockServ.Object) { CallBase = true };
            controller.SetupGet(p => p.IsAdmin).Returns(true);

            //act
            var result = await controller.Object.GetRule(id);

            //assert
            var ruleOk = Assert.IsType<OkObjectResult>(result);
            var modelOk = Assert.IsType<RuleAdminDTO>(ruleOk.Value);
            Assert.Equal("LibraryRule", modelOk.Title);
            Assert.Equal(20, modelOk.MinTime);
        }

        [Theory]
        [InlineData(20)]
        public async Task GetRuleForAdminReturnsError(int id)
        {
            //arrange
            mockServ.Setup(p => p.Get(id)).ReturnsAsync((Rule)null);
            var controller = new Mock<RuleController>(mockServ.Object) { CallBase = true };
            controller.SetupGet(p => p.IsAdmin).Returns(true);

            //act
            var result = await controller.Object.GetRule(id);

            //assert
            var ruleBad = Assert.IsType<OkObjectResult>(result);
            Assert.Null(ruleBad.Value);
        }


        [Theory]
        [InlineData(3)]
        public async Task GetRuleForUser(int id)
        {
            //arrange
            mockServ.Setup(p => p.Get(id)).ReturnsAsync(initRules().Single(p => p.Id == id));
            var controller = new Mock<RuleController>(mockServ.Object) { CallBase = true };
            controller.SetupGet(p => p.existsActive).Returns(true);
            controller.SetupGet(p => p.IsAdmin).Returns(false);


            //act
            var result = await controller.Object.GetRule(id);

            //assert
            var ruleOk = Assert.IsType<OkObjectResult>(result);
            var modelOk = Assert.IsType<RuleBasicDTO>(ruleOk.Value);
            Assert.Equal("BunkerRule", modelOk.Title);
            Assert.Equal(30, modelOk.MinTime);
        }

        [Theory]
        [InlineData(30)]
        public async Task GetRuleForUserReturnsError(int id)
        {
            //arrange
            mockServ.Setup(p => p.Get(id)).ReturnsAsync((Rule)null);
            var controller = new Mock<RuleController>(mockServ.Object) { CallBase = true };
            controller.SetupGet(p => p.existsActive).Returns(false);
            controller.SetupGet(p => p.IsAdmin).Returns(false);

            //act
            var result = await controller.Object.GetRule(id);

            //assert
            var ruleBad = Assert.IsType<BadRequestResult>(result);
        }
        #endregion

        #region CreateRule
        [Fact]
        public async Task CreateRuleWithInvalidModel()
        {
            //arrange
            mockServ.Setup(p => p.Create(It.IsAny<Rule>()));
            var controller = new Mock<RuleController>(mockServ.Object) { CallBase = true };
            controller.Object.ModelState.AddModelError("error", "someeror");

            //act
            var result = await controller.Object.CreateRule(It.IsAny<RuleDetailedDTO>());

            //assert
            var ruleOk = Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task CreateRule()
        {
            //arrange
            mockServ.Setup(p => p.Create(someRule()));
            var controller = new Mock<RuleController>(mockServ.Object) { CallBase = true };
            controller.SetupGet(p => p.UserId).Returns(It.IsAny<string>);

            //act
            var result = await controller.Object.CreateRule(someDTORule());

            //assert
            var ruleOk = Assert.IsType<OkObjectResult>(result);
            var ruleModel = Assert.IsType<Rule>(ruleOk.Value);
            Assert.Equal("ComputerRule", ruleModel.Title);
        }
        #endregion

        #region Delete Rule
        [Theory]
        [InlineData(1)]
        public async Task DeleteRule(int id)
        {
            //arrange
            mockServ.Setup(p => p.Delete(id)).Returns(Task.CompletedTask);
            var controller = new Mock<RuleController>(mockServ.Object) { CallBase = true };

            //act
            var result = await controller.Object.DeleteRule(id);

            //assert
            var ruleOk = Assert.IsType<OkObjectResult>(result);

        }

        [Fact]
        public async Task DeleteRuleRelativeToResourcesReturnsError()
        {
            //arrange
            mockServ.Setup(p => p.Delete(It.IsAny<int>()));
            var controller = new Mock<RuleController>(mockServ.Object) { CallBase = true };
            controller.SetReturnsDefault(HttpStatusCode.Forbidden);

            //act
            var result =  controller.Object.DeleteRule(It.IsAny<int>()).IsFaulted;

            //assert
            var ruleOk = Assert.IsType<bool> (result);
            Assert.True(result);

        }
        #endregion

        #region Update Rule
        [Fact]
        public async Task UpdateRuleWithBadModelReturnsBadRequest()
        {
            //arrange
            mockServ.Setup(f => f.Update(someRule())).Returns(Task.CompletedTask);
            var controller = new Mock<RuleController>(mockServ.Object) { CallBase = true };
            controller.Object.ModelState.AddModelError("error", "smth");
            //act
            var result = await controller.Object.UpdateRule(1, someDTORule());

            //Assert
            var ruleBad = Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task UpdateRule()
        {
            //arrange
            mockServ.Setup(f => f.Update(someRule())).Returns(Task.CompletedTask);
            var controller = new Mock<RuleController>(mockServ.Object) { CallBase = true };
            controller.SetupGet(p => p.UserId).Returns(It.IsAny<string>());
            //act
            var result = await controller.Object.UpdateRule(1, someDTORule());

            //Assert
            var ruleOk = Assert.IsType<OkObjectResult>(result);
            var ruleModel = Assert.IsType<string>(ruleOk.Value);
            Assert.Equal("Rule's been updated", ruleOk.Value);
        }
        #endregion

        #region TestDataHelper
        public IEnumerable<Rule> initRules()
        {
            var rules = new List<Rule>();
            rules.Add(new Rule
            {
                Id = 1,
                IsActive = true,
                Title = "ComputerRule",
                CreatedTime = DateTime.Now,
                UpdatedTime = DateTime.Now,
                CreatedUserId = "1",
                UpdatedUserId = "1",
                MinTime = 10,
                MaxTime = 100
            });
            rules.Add(new Rule
            {
                Id = 2,
                IsActive = true,
                Title = "LibraryRule",
                CreatedTime = DateTime.Now,
                UpdatedTime = DateTime.Now,
                CreatedUserId = "2",
                UpdatedUserId = "2",
                MinTime = 20,
                MaxTime = 200
            });
            rules.Add(new Rule
            {
                Id = 3,
                IsActive = false,
                Title = "BunkerRule",
                CreatedTime = DateTime.Now,
                UpdatedTime = DateTime.Now,
                CreatedUserId = "3",
                UpdatedUserId = "3",
                MinTime = 30,
                MaxTime = 300
            });
            return rules;
        }

        public Rule someRule()
        {
            return new Rule
            {
                Id = 1,
                IsActive = true,
                Title = "ComputerRule",
                CreatedTime = DateTime.Now,
                UpdatedTime = DateTime.Now,
                CreatedUserId = "1",
                UpdatedUserId = "1",
                MinTime = 10,
                MaxTime = 100
            };
        }

        public RuleDetailedDTO someDTORule()
        {
            return new RuleDetailedDTO
            {
                IsActive = true,
                Title = "ComputerRule",
                MinTime = 10,
                MaxTime = 100
            };
        }
#endregion

    }
}
