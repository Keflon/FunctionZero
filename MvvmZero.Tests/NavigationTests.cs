using FunctionZero.Maui.MvvmZero;
using FunctionZero.Maui.MvvmZero.PageControllers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Hosting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MvvmZero.Tests;

[TestClass]
public class NavigationTests
{
    [TestMethod]
    public void MapVmToView_WhenMappingAlreadyExists_Throws()
    {
        var builder = CreateBuilder();
        builder.MapVmToView<object>(_ => new ContentView());

        Assert.ThrowsExactly<InvalidOperationException>(() =>
            builder.MapVmToView<object>(_ => new ContentView()));
    }

    [TestMethod]
    public void Build_WhenCalledTwice_Throws()
    {
        var builder = CreateBuilder();
        _ = builder.Build();

        Assert.ThrowsExactly<InvalidOperationException>(() => builder.Build());
    }

    [TestMethod]
    public async Task PopToRootAsync_ForModalStack_PreservesModalRoot()
    {
        var navigation = new FakeNavigation();
        navigation.ModalPages.AddRange([null!, null!, null!]);
        var finderCalls = 0;
        var service = CreatePageService(() =>
        {
            finderCalls++;
            return navigation;
        });

        await service.PopToRootAsync(isModal: true, animated: false);

        Assert.AreEqual(1, navigation.ModalStack.Count);
        Assert.AreEqual(2, navigation.PopModalCalls);
        Assert.AreEqual(1, finderCalls);
    }

    [TestMethod]
    public void FindAncestorPageVm_WhenNavigationIsMissing_ReturnsNull()
    {
        var service = CreatePageService(() => null!);

        var viewModel = service.FindAncestorPageVm<object>();

        Assert.IsNull(viewModel);
    }

    [TestMethod]
    public void MultiPageController_WhenMultiPageIsMissing_IsNullSafe()
    {
        var controller = new MultiPageController(() => null);

        Assert.IsFalse(controller.HasMultiPage);
        Assert.IsNull(controller.ItemsSource);
        Assert.IsNull(controller.SelectedItem);
        Assert.ThrowsExactly<InvalidOperationException>(() => controller.SelectedItem = new object());
    }

    [TestMethod]
    public void UseMvvmZero_RegistersThePageServicesFlyoutController()
    {
        var mauiBuilder = MauiApp.CreateBuilder().UseMvvmZero();
        using var provider = mauiBuilder.Services.BuildServiceProvider();

        var pageService = provider.GetRequiredService<IPageServiceZero>();
        var flyoutController = provider.GetRequiredService<IFlyoutController>();

        Assert.AreSame(pageService.FlyoutController, flyoutController);
    }

    private static PageServiceBuilder CreateBuilder()
    {
        var navigation = new FakeNavigation();
        return new PageServiceBuilder(() => navigation, () => null)
            .SetTypeFactory(type => Activator.CreateInstance(type)
                ?? throw new InvalidOperationException($"Cannot create {type}."));
    }

    private static PageServiceZero CreatePageService(Func<INavigation> navigationFinder)
    {
        return new PageServiceZero(
            type => Activator.CreateInstance(type)
                ?? throw new InvalidOperationException($"Cannot create {type}."),
            () => new FlyoutPage(),
            navigationFinder,
            () => null,
            (_, _) => new ContentView());
    }

    private sealed class FakeNavigation : INavigation
    {
        public List<Page> ModalPages { get; } = [];
        public List<Page> NavigationPages { get; } = [];
        public IReadOnlyList<Page> ModalStack => ModalPages;
        public IReadOnlyList<Page> NavigationStack => NavigationPages;
        public int PopModalCalls { get; private set; }

        public void InsertPageBefore(Page page, Page before)
        {
            NavigationPages.Insert(NavigationPages.IndexOf(before), page);
        }

        public Task<Page?> PopAsync() => PopAsync(true);

        public Task<Page?> PopAsync(bool animated)
        {
            if (NavigationPages.Count == 0)
                return Task.FromResult<Page?>(null);

            var page = NavigationPages[^1];
            NavigationPages.RemoveAt(NavigationPages.Count - 1);
            return Task.FromResult<Page?>(page);
        }

        public Task<Page?> PopModalAsync() => PopModalAsync(true);

        public Task<Page?> PopModalAsync(bool animated)
        {
            PopModalCalls++;
            if (ModalPages.Count == 0)
                return Task.FromResult<Page?>(null);

            var page = ModalPages[^1];
            ModalPages.RemoveAt(ModalPages.Count - 1);
            return Task.FromResult<Page?>(page);
        }

        public Task PopToRootAsync() => PopToRootAsync(true);

        public Task PopToRootAsync(bool animated)
        {
            if (NavigationPages.Count > 1)
                NavigationPages.RemoveRange(1, NavigationPages.Count - 1);
            return Task.CompletedTask;
        }

        public Task PushAsync(Page page) => PushAsync(page, true);

        public Task PushAsync(Page page, bool animated)
        {
            NavigationPages.Add(page);
            return Task.CompletedTask;
        }

        public Task PushModalAsync(Page page) => PushModalAsync(page, true);

        public Task PushModalAsync(Page page, bool animated)
        {
            ModalPages.Add(page);
            return Task.CompletedTask;
        }

        public void RemovePage(Page page)
        {
            NavigationPages.Remove(page);
        }
    }
}