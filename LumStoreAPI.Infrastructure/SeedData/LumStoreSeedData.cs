using LumStoreAPI.Core.Entities.DocumentEngine;
using LumStoreAPI.Core.Entities.DocumentTypes;
using LumStoreAPI.Core.Entities.Pages;
using LumStoreAPI.Core.Entities.Systems;
using LumStoreAPI.Core.Models.Enums;
using LumStoreAPI.Libraries.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace LumStoreAPI.Infrastructure.SeedData
{
    public static class LumStoreSeedData
    {
        public static async Task SeedAsync(IServiceProvider services)
        {
            using var scope = services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<LumStoreContext>();
            var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>()
                             .CreateLogger(nameof(LumStoreSeedData));

            try
            {
                if (await context.Users.AnyAsync())
                {
                    logger.LogInformation("Seed data already exists. Skipping.");
                    return;
                }

                logger.LogInformation("Seeding LUM Nails database...");

                await SeedUsersAsync(context);
                await SeedSettingsAsync(context);
                //await SeedMediaLibraryAsync(context);
                //await SeedDocumentTreeAsync(context);

                logger.LogInformation("LUM Nails database seeded successfully.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while seeding the database.");
                throw;
            }
        }

        // ─── Users ────────────────────────────────────────────────────────────────

        private static async Task SeedUsersAsync(LumStoreContext context)
        {
            var users = new List<User>
            {
                new()
                {
                    UserName     = "admin@lumnails.com",
                    UserPassword = HashHelper.HashPassword("123456@#"),
                    FirstName    = "Admin",
                    LastName     = "LUM",
                    Email        = "admin@lumnails.com",
                    IsAdmin      = true,
                    IsVerified   = true,
                    IsEnabled    = true,
                    IsLocked     = false,
                    UserLevel    = 1,
                    TimeLocked   = DateTimeOffset.UtcNow
                },
                new()
                {
                    UserName     = "sarah@lumnails.com",
                    UserPassword = HashHelper.HashPassword("123456@#"),
                    FirstName    = "Sarah",
                    LastName     = "Nguyen",
                    Email        = "sarah@lumnails.com",
                    IsAdmin      = false,
                    IsVerified   = true,
                    IsEnabled    = true,
                    IsLocked     = false,
                    UserLevel    = 2,
                    TimeLocked   = DateTimeOffset.UtcNow
                },
                new()
                {
                    UserName     = "emily@lumnails.com",
                    UserPassword = HashHelper.HashPassword("123456@#"),
                    FirstName    = "Emily",
                    LastName     = "Tran",
                    Email        = "emily@lumnails.com",
                    IsAdmin      = false,
                    IsVerified   = true,
                    IsEnabled    = true,
                    IsLocked     = false,
                    UserLevel    = 2,
                    TimeLocked   = DateTimeOffset.UtcNow
                }
            };

            await context.Users.AddRangeAsync(users);
            await context.SaveChangesAsync();
        }

        // ─── Settings ─────────────────────────────────────────────────────────────

        private static async Task SeedSettingsAsync(LumStoreContext context)
        {
            var settings = new List<SettingKeyValue>
            {
                new() { SettingCode = "SITE_NAME",       SettingName = "Site Name",           SettingValue = "LUM Nails" },
                new() { SettingCode = "SITE_URL",        SettingName = "Site URL",            SettingValue = "http://localhost:3000" },
                new() { SettingCode = "CONTACT_EMAIL",   SettingName = "Contact Email",       SettingValue = "hello@lumnails.com" },
                new() { SettingCode = "SMTP_FROM",       SettingName = "SMTP From Address",   SettingValue = "noreply@lumnails.com" },
                new() { SettingCode = "CURRENCY",        SettingName = "Default Currency",    SettingValue = "USD" },
                new() { SettingCode = "CURRENCY_SYMBOL", SettingName = "Currency Symbol",     SettingValue = "$" },
                new() { SettingCode = "INSTAGRAM_URL",   SettingName = "Instagram URL",       SettingValue = "https://instagram.com/lumnails" },
                new() { SettingCode = "TIKTOK_URL",      SettingName = "TikTok URL",          SettingValue = "https://tiktok.com/@lumnails" },
                new() { SettingCode = "FREE_SHIPPING",   SettingName = "Free Shipping Over",  SettingValue = "50" },
                new() { SettingCode = "TAX_RATE",        SettingName = "Tax Rate (%)",        SettingValue = "8" }
            };

            await context.SettingKeyValues.AddRangeAsync(settings);
            await context.SaveChangesAsync();
        }

        // ─── Media Library ────────────────────────────────────────────────────────

        private static async Task SeedMediaLibraryAsync(LumStoreContext context)
        {
            var categories = new List<MediaLibraryCategory>
            {
                new() { CategoryName = "Products",   FolderName = "products"   },
                new() { CategoryName = "Banners",    FolderName = "banners"    },
                new() { CategoryName = "Gel Polish", FolderName = "gel-polish" },
                new() { CategoryName = "Nail Art",   FolderName = "nail-art"   },
                new() { CategoryName = "Tools",      FolderName = "tools"      },
                new() { CategoryName = "Care",       FolderName = "care"       },
                new() { CategoryName = "General",    FolderName = "general"    }
            };

            await context.MediaLibraryCategories.AddRangeAsync(categories);
            await context.SaveChangesAsync();
        }

        // ─── Document Tree ────────────────────────────────────────────────────────

        private static async Task SeedDocumentTreeAsync(LumStoreContext context)
        {
            // ── Hero slides JSON (matches lum-nails heroSlides mock data exactly) ─

            var heroSlidesJson = JsonSerializer.Serialize(new[]
            {
                new {
                    pretitle         = "NEW ARRIVAL",
                    title            = "NAIL ART BRUSHES",
                    description      = "Precision tools for every technique. Elevate your artistry with our premium collection designed for professionals.",
                    image            = "/images/NAB-HP-HERO 1.png",
                    primaryCtaText   = "Shop Collection",
                    primaryCtaLink   = "/collection/tools",
                    secondaryCtaText = "View Tutorial",
                    secondaryCtaLink = "/news"
                },
                new {
                    pretitle         = "BEST SELLER",
                    title            = "GEL POLISH KITS",
                    description      = "Everything you need for a salon-quality manicure at home. Long-lasting, vibrant colors that shine.",
                    image            = "/images/NAB-HP-HERO 1.png",
                    primaryCtaText   = "Shop Kits",
                    primaryCtaLink   = "/collection/gel-polish",
                    secondaryCtaText = "Compare",
                    secondaryCtaLink = "/shop"
                },
                new {
                    pretitle         = "LIMITED EDITION",
                    title            = "SUMMER VIBES",
                    description      = "Capture the essence of summer with our exclusive palette. Bright, bold, and beautiful shades.",
                    image            = "/images/NAB-HP-HERO 1.png",
                    primaryCtaText   = "Shop Summer",
                    primaryCtaLink   = "/collection/gel-polish",
                    secondaryCtaText = "Lookbook",
                    secondaryCtaLink = "/news"
                },
                new {
                    pretitle         = "SKINCARE",
                    title            = "HAND & NAIL CARE",
                    description      = "Nourish your hands and cuticles with our organic oils and creams. The perfect finish to any manicure.",
                    image            = "/images/NAB-HP-HERO 1.png",
                    primaryCtaText   = "Shop Care",
                    primaryCtaLink   = "/collection/care",
                    secondaryCtaText = "Ingredients",
                    secondaryCtaLink = "/about"
                }
            });

            // ── Level 0: Root ──────────────────────────────────────────────────

            var root = new DocumentNode { NodeName = "Root", NodeAlias = "root", RelativeUrl = "/", NodeOrder = 1 };
            await context.DocumentNodes.AddAsync(root);
            await context.SaveChangesAsync();

            // ── Level 1: Main pages + 6 categories directly under root ─────────

            var home = new DocumentNode { NodeName = "Home", NodeAlias = "home", RelativeUrl = "/home", NodeOrder = 1, ParentNodeID = root.NodeID };
            var catGel = new DocumentNode { NodeName = "Gel Polish", NodeAlias = "gel-polish", RelativeUrl = "/gel-polish", NodeOrder = 2, ParentNodeID = root.NodeID };
            var catNailArt = new DocumentNode { NodeName = "Nail Art", NodeAlias = "nail-art", RelativeUrl = "/nail-art", NodeOrder = 3, ParentNodeID = root.NodeID };
            var catBundles = new DocumentNode { NodeName = "Bundles", NodeAlias = "bundles", RelativeUrl = "/bundles", NodeOrder = 4, ParentNodeID = root.NodeID };
            var catTools = new DocumentNode { NodeName = "Tools", NodeAlias = "tools", RelativeUrl = "/tools", NodeOrder = 5, ParentNodeID = root.NodeID };
            var catCare = new DocumentNode { NodeName = "Care", NodeAlias = "care", RelativeUrl = "/care", NodeOrder = 6, ParentNodeID = root.NodeID };
            var catHoliday = new DocumentNode { NodeName = "Holiday", NodeAlias = "holiday", RelativeUrl = "/holiday", NodeOrder = 7, ParentNodeID = root.NodeID };

            await context.DocumentNodes.AddRangeAsync(home, catGel, catNailArt, catBundles, catTools, catCare, catHoliday);
            await context.SaveChangesAsync();

            // ── Level 2: 12 product nodes under their categories ───────────────

            // Gel Polish — 4 products
            var nodeBlackWhite = new DocumentNode { NodeName = "Black White Gel Duo", NodeAlias = "black-white-gel-duo", RelativeUrl = "/gel-polish/black-white-gel-duo", NodeOrder = 1, ParentNodeID = catGel.NodeID };
            var nodeNude = new DocumentNode { NodeName = "Nude Collection Gel Polish", NodeAlias = "nude-collection-gel-polish", RelativeUrl = "/gel-polish/nude-collection-gel-polish", NodeOrder = 2, ParentNodeID = catGel.NodeID };
            var nodeNeon = new DocumentNode { NodeName = "Neon Summer Collection", NodeAlias = "neon-summer-collection", RelativeUrl = "/gel-polish/neon-summer-collection", NodeOrder = 3, ParentNodeID = catGel.NodeID };
            var nodeTopCoat = new DocumentNode { NodeName = "Gel Top Coat Shiny", NodeAlias = "gel-top-coat-shiny", RelativeUrl = "/gel-polish/gel-top-coat-shiny", NodeOrder = 4, ParentNodeID = catGel.NodeID };

            // Nail Art — 1 product
            var nodeChrome = new DocumentNode { NodeName = "Chrome Powder Kit", NodeAlias = "chrome-powder-kit", RelativeUrl = "/nail-art/chrome-powder-kit", NodeOrder = 1, ParentNodeID = catNailArt.NodeID };

            // Bundles — 2 products
            var nodeHalloween = new DocumentNode { NodeName = "V2 Halloween Bundle", NodeAlias = "v2-halloween-bundle", RelativeUrl = "/bundles/v2-halloween-bundle", NodeOrder = 1, ParentNodeID = catBundles.NodeID };
            var nodeStarterKit = new DocumentNode { NodeName = "Gel Starter Kit", NodeAlias = "gel-starter-kit", RelativeUrl = "/bundles/gel-starter-kit", NodeOrder = 2, ParentNodeID = catBundles.NodeID };

            // Tools — 2 products
            var nodeBrushSet = new DocumentNode { NodeName = "Precision Nail Art Brush Set", NodeAlias = "precision-nail-art-brush-set", RelativeUrl = "/tools/precision-nail-art-brush-set", NodeOrder = 1, ParentNodeID = catTools.NodeID };
            var nodeFileSet = new DocumentNode { NodeName = "Nail File Buffer Set", NodeAlias = "nail-file-buffer-set", RelativeUrl = "/tools/nail-file-buffer-set", NodeOrder = 2, ParentNodeID = catTools.NodeID };

            // Care — 2 products
            var nodeCuticle = new DocumentNode { NodeName = "Cuticle Oil Trio", NodeAlias = "cuticle-oil-trio", RelativeUrl = "/care/cuticle-oil-trio", NodeOrder = 1, ParentNodeID = catCare.NodeID };
            var nodeHandCream = new DocumentNode { NodeName = "Hand Cream Luxury", NodeAlias = "hand-cream-luxury", RelativeUrl = "/care/hand-cream-luxury", NodeOrder = 2, ParentNodeID = catCare.NodeID };

            // Holiday — 1 product
            var nodeHoliday = new DocumentNode { NodeName = "Holiday Bundle", NodeAlias = "holiday-bundle", RelativeUrl = "/holiday/holiday-bundle", NodeOrder = 1, ParentNodeID = catHoliday.NodeID };

            await context.DocumentNodes.AddRangeAsync(
                nodeBlackWhite, nodeNude, nodeNeon, nodeTopCoat,
                nodeChrome,
                nodeHalloween, nodeStarterKit,
                nodeBrushSet, nodeFileSet,
                nodeCuticle, nodeHandCream,
                nodeHoliday
            );
            await context.SaveChangesAsync();

            // ── Closure table ──────────────────────────────────────────────────

            var nodeTree = new (int NodeID, int? ParentNodeID)[]
            {
                (root.NodeID,            null),
                (home.NodeID,            root.NodeID),
                (catGel.NodeID,          root.NodeID),
                (catNailArt.NodeID,      root.NodeID),
                (catBundles.NodeID,      root.NodeID),
                (catTools.NodeID,        root.NodeID),
                (catCare.NodeID,         root.NodeID),
                (catHoliday.NodeID,      root.NodeID),
                (nodeBlackWhite.NodeID,  catGel.NodeID),
                (nodeNude.NodeID,        catGel.NodeID),
                (nodeNeon.NodeID,        catGel.NodeID),
                (nodeTopCoat.NodeID,     catGel.NodeID),
                (nodeChrome.NodeID,      catNailArt.NodeID),
                (nodeHalloween.NodeID,   catBundles.NodeID),
                (nodeStarterKit.NodeID,  catBundles.NodeID),
                (nodeBrushSet.NodeID,    catTools.NodeID),
                (nodeFileSet.NodeID,     catTools.NodeID),
                (nodeCuticle.NodeID,     catCare.NodeID),
                (nodeHandCream.NodeID,   catCare.NodeID),
                (nodeHoliday.NodeID,     catHoliday.NodeID),
            };

            await context.DocumentLinkedNodes.AddRangeAsync(BuildClosureTable(nodeTree));
            await context.SaveChangesAsync();

            // ── Document Pages ─────────────────────────────────────────────────

            // Root folder (no page type)
            await context.DocumentPages.AddAsync(
                new DocumentPage { NodeID = root.NodeID, DocumentName = "Root" }
            );

            // Home page with hero slides JSON
            await context.HomePages.AddAsync(new HomePage
            {
                NodeID = home.NodeID,
                DocumentName = "Home",
                PageTitle = "LUM Nails — Premium Nail Beauty",
                Description = "Discover professional-grade nail products for salon-quality results at home.",
            });

            // ── ProductCategory pages (match lum-nails categories exactly) ─────

            await context.ProductCategories.AddRangeAsync(
                new ProductCategory
                {
                    NodeID = catGel.NodeID,
                    DocumentName = "Gel Polish",
                    CategoryName = "Gel Polish",
                    CategoryImage = "https://images.unsplash.com/photo-1604654894610-df63bc536371?auto=format&fit=crop&q=80&w=600",
                    CategoryDescription = "Long-lasting gel polishes in every shade. Chip-resistant, vibrant colors that cure under LED/UV lamp."
                },
                new ProductCategory
                {
                    NodeID = catNailArt.NodeID,
                    DocumentName = "Nail Art",
                    CategoryName = "Nail Art",
                    CategoryImage = "https://images.unsplash.com/photo-1604654894610-df63bc536371?auto=format&fit=crop&q=80&w=600",
                    CategoryDescription = "Chrome powders, stamping kits, nail stickers and art supplies for creative nail designs."
                },
                new ProductCategory
                {
                    NodeID = catBundles.NodeID,
                    DocumentName = "Bundles",
                    CategoryName = "Bundles",
                    CategoryImage = "https://images.unsplash.com/photo-1522337660859-02fbefca4702?auto=format&fit=crop&q=80&w=600",
                    CategoryDescription = "Curated sets and starter kits for every skill level. Save more when you bundle."
                },
                new ProductCategory
                {
                    NodeID = catTools.NodeID,
                    DocumentName = "Tools & Brushes",
                    CategoryName = "Tools & Brushes",
                    CategoryImage = "https://images.unsplash.com/photo-1629198688000-71f23e745b6e?auto=format&fit=crop&q=80&w=600",
                    CategoryDescription = "Professional-grade nail art brushes, files, buffers, and tools for salon-quality results."
                },
                new ProductCategory
                {
                    NodeID = catCare.NodeID,
                    DocumentName = "Hand & Nail Care",
                    CategoryName = "Hand & Nail Care",
                    CategoryImage = "https://images.unsplash.com/photo-1519014816548-bf5fe059e98b?auto=format&fit=crop&q=80&w=600",
                    CategoryDescription = "Nourishing cuticle oils, hand creams, and strengthening treatments for healthy nails."
                },
                new ProductCategory
                {
                    NodeID = catHoliday.NodeID,
                    DocumentName = "Holiday",
                    CategoryName = "Holiday",
                    CategoryImage = "https://images.unsplash.com/photo-1599305090598-fe179d501227?auto=format&fit=crop&q=80&w=600",
                    CategoryDescription = "Limited edition holiday collections and gift sets. Perfect for gifting or treating yourself."
                }
            );

            await context.SaveChangesAsync();

            // ── Products ───────────────────────────────────────────────────────
            //
            // Price         = original / list price   (lum-nails: oldPrice)
            // PriceDiscount = discount amount          (oldPrice - salePrice)
            // Sale price    = Price - PriceDiscount    (lum-nails: price)
            //
            // isSale is derived: PriceDiscount > 0
            // All nail products → ProductGroup.SORT_GOODS

            // ── Gel Polish products ────────────────────────────────────────────

            var prodBlackWhite = new Product
            {
                NodeID = nodeBlackWhite.NodeID,
                DocumentName = "Black + White Gel Color Duo",
                ProductName = "Black + White Gel Color Duo",
                ProductType = ProductType.SIMPLE,
                ProductGroup = ProductGroup.SORT_GOODS,
                ShortDescription = "The timeless duo — pure black and pure white gel polishes.",
                Description = "<p>The timeless duo. Pure black and pure white — the two shades every nail collection needs. Highly pigmented, chip-resistant formula that lasts up to 3 weeks.</p>",
                Price = 38m,
                PriceDiscount = 0m,
                IsNew = false,
                IsBestSeller = true,
                Tags = ["gel", "duo", "classic"],
                Rating = 4.7,
                ReviewCount = 213,
                Weight = 0.1,
                Images = []
            };

            var prodNude = new Product
            {
                NodeID = nodeNude.NodeID,
                DocumentName = "Nude Collection — 6pc Gel Polish",
                ProductName = "Nude Collection — 6pc Gel Polish",
                ProductType = ProductType.SIMPLE,
                ProductGroup = ProductGroup.SORT_GOODS,
                ShortDescription = "Six perfectly curated nude shades for every skin tone.",
                Description = "<p>Six perfectly curated nude shades for every skin tone. From porcelain to deep caramel — timeless and elegant. Long-wear formula, no chipping for up to 21 days.</p>",
                Price = 72m,
                PriceDiscount = 0m,
                IsNew = false,
                IsBestSeller = true,
                Tags = ["gel", "nude", "collection"],
                Rating = 4.6,
                ReviewCount = 175,
                Weight = 0.3,
                Images = []
            };

            var prodNeon = new Product
            {
                NodeID = nodeNeon.NodeID,
                DocumentName = "Neon Summer — 4pc Collection",
                ProductName = "Neon Summer — 4pc Collection",
                ProductType = ProductType.SIMPLE,
                ProductGroup = ProductGroup.SORT_GOODS,
                ShortDescription = "Four electric neon shades that scream summer.",
                Description = "<p>Four electric neon shades that scream summer. UV-reactive formula glows under blacklight. Coral, yellow, hot pink, and lime green. Perfect for festivals and beach days.</p>",
                Price = 52m,
                PriceDiscount = 0m,
                IsNew = true,
                IsBestSeller = false,
                Tags = ["gel", "neon", "summer", "collection"],
                Rating = 4.5,
                ReviewCount = 63,
                Weight = 0.2,
                Images = []
            };

            var prodTopCoat = new Product
            {
                NodeID = nodeTopCoat.NodeID,
                DocumentName = "Ultra-Shine Gel Top Coat",

                ProductName = "Ultra-Shine Gel Top Coat",
                ProductType = ProductType.SIMPLE,
                ProductGroup = ProductGroup.SORT_GOODS,
                ShortDescription = "Mirror-like shine that lasts up to 4 weeks.",
                Description = "<p>Mirror-like shine that lasts up to 4 weeks. Self-leveling formula eliminates brush strokes. Compatible with all gel polish brands. No-wipe finish for a flawless result every time.</p>",
                Price = 22m,
                PriceDiscount = 0m,
                IsNew = false,
                IsBestSeller = true,
                Tags = ["gel", "top-coat", "finish"],
                Rating = 4.7,
                ReviewCount = 289,
                Weight = 0.08,
                Images = []
            };

            // ── Nail Art products ──────────────────────────────────────────────

            var prodChrome = new Product
            {
                NodeID = nodeChrome.NodeID,
                DocumentName = "Chrome Powder Mirror Kit",

                ProductName = "Chrome Powder Mirror Kit",
                ProductType = ProductType.SIMPLE,
                ProductGroup = ProductGroup.SORT_GOODS,
                ShortDescription = "Achieve a perfect mirror chrome finish — 5-piece kit.",
                Description = "<p>Achieve a perfect mirror chrome finish with this 5-piece kit. Includes gold, silver, rose gold, purple, and blue chrome powders. Works over any gel top coat.</p>",
                Price = 42m,
                PriceDiscount = 8m,       // sale: $34
                IsNew = false,
                IsBestSeller = false,
                Tags = ["nail-art", "chrome", "powder"],
                Rating = 4.6,
                ReviewCount = 118,
                Weight = 0.1,
                Images = []
            };

            // ── Bundle products ────────────────────────────────────────────────

            var prodHalloween = new Product
            {
                NodeID = nodeHalloween.NodeID,
                DocumentName = "V.2 Halloween Bundle",

                ProductName = "V.2 Halloween Bundle",
                ProductType = ProductType.BUNDLE,
                ProductGroup = ProductGroup.SORT_GOODS,
                ShortDescription = "The ultimate Halloween nail kit — 6 gel polishes + top coat + base coat + stickers.",
                Description = "<p>The ultimate Halloween nail kit. Includes 6 gel polishes in classic Halloween shades (black, orange, blood red, deep purple, ghost white, forest green), top coat, base coat, and nail art stickers. Everything you need for spooky season nails.</p>",
                Price = 170m,
                PriceDiscount = 63m,      // sale: $107
                IsNew = false,
                IsBestSeller = true,
                Tags = ["bundle", "halloween", "sale"],
                Rating = 4.8,
                ReviewCount = 142,
                Weight = 0.6,
                IsNeedBox = true,
                Images = []
            };

            var prodStarterKit = new Product
            {
                NodeID = nodeStarterKit.NodeID,
                DocumentName = "Complete Gel Starter Kit",

                ProductName = "Complete Gel Starter Kit",
                ProductType = ProductType.BUNDLE,
                ProductGroup = ProductGroup.SORT_GOODS,
                ShortDescription = "Everything you need to start gel nails at home — LED lamp included.",
                Description = "<p>Everything you need to get started with gel nails at home. Includes 36W LED lamp, base coat, top coat, 4 gel polish shades (nude, pink, red, clear), nail file, and cuticle pusher. Salon-quality results from day one.</p>",
                Price = 120m,
                PriceDiscount = 31m,      // sale: $89
                IsNew = true,
                IsBestSeller = true,
                Tags = ["bundle", "starter", "gel", "kit"],
                Rating = 4.9,
                ReviewCount = 412,
                Weight = 1.2,
                IsNeedBox = true,
                Images = []
            };

            // ── Tools products ─────────────────────────────────────────────────

            var prodBrushSet = new Product
            {
                NodeID = nodeBrushSet.NodeID,
                DocumentName = "Precision Nail Art Brush Set",

                ProductName = "Precision Nail Art Brush Set",
                ProductType = ProductType.SIMPLE,
                ProductGroup = ProductGroup.SORT_GOODS,
                ShortDescription = "8 professional-grade brushes for every nail art technique.",
                Description = "<p>8 professional-grade brushes for every nail art technique. Ultra-fine liner, fan brush, flat brush, dotter, and more. Ergonomic handles for maximum control. Suitable for both gel and regular polish.</p>",
                Price = 60m,
                PriceDiscount = 15m,      // sale: $45
                IsNew = true,
                IsBestSeller = false,
                Tags = ["brush", "nail-art", "tools"],
                Rating = 4.9,
                ReviewCount = 328,
                Weight = 0.15,
                Images = []
            };

            var prodFileSet = new Product
            {
                NodeID = nodeFileSet.NodeID,
                DocumentName = "Professional Nail File & Buffer Set",

                ProductName = "Professional Nail File & Buffer Set",
                ProductType = ProductType.SIMPLE,
                ProductGroup = ProductGroup.SORT_GOODS,
                ShortDescription = "Five-piece professional nail file and buffer set.",
                Description = "<p>Five-piece professional nail file and buffer set. Multiple grits for shaping, smoothing, and adding shine. Includes 80/80, 100/180, 220 grit files and a 4-way buffer block. Salon-grade quality.</p>",
                Price = 18m,
                PriceDiscount = 0m,
                IsNew = false,
                IsBestSeller = false,
                Tags = ["tools", "file", "buffer"],
                Rating = 4.4,
                ReviewCount = 57,
                Weight = 0.12,
                Images = []
            };

            // ── Hand & Nail Care products ──────────────────────────────────────

            var prodCuticle = new Product
            {
                NodeID = nodeCuticle.NodeID,
                DocumentName = "Nourishing Cuticle Oil Trio",

                ProductName = "Nourishing Cuticle Oil Trio",
                ProductType = ProductType.SIMPLE,
                ProductGroup = ProductGroup.SORT_GOODS,
                ShortDescription = "Three luxurious cuticle oils — vitamin E, jojoba, and rose hip.",
                Description = "<p>Three luxurious cuticle oils infused with vitamin E, jojoba, and rose hip. Deeply hydrating, fast-absorbing formula. Promotes healthy nail growth, softens cuticles, and reduces hangnails. Available in lavender, citrus, and unscented.</p>",
                Price = 29m,
                PriceDiscount = 0m,
                IsNew = false,
                IsBestSeller = false,
                Tags = ["care", "cuticle", "oil"],
                Rating = 4.8,
                ReviewCount = 96,
                Weight = 0.18,
                Images = []
            };

            var prodHandCream = new Product
            {
                NodeID = nodeHandCream.NodeID,
                DocumentName = "Luxury Hand Cream — Rose & Argan",

                ProductName = "Luxury Hand Cream — Rose & Argan",
                ProductType = ProductType.SIMPLE,
                ProductGroup = ProductGroup.SORT_GOODS,
                ShortDescription = "Rich hand cream with rose extract and Moroccan argan oil.",
                Description = "<p>Rich, fast-absorbing hand cream with rose extract and Moroccan argan oil. Non-greasy formula strengthens nails and softens skin. Dermatologist tested. Vegan and cruelty-free. 75ml tube.</p>",
                Price = 24m,
                PriceDiscount = 0m,
                IsNew = false,
                IsBestSeller = false,
                Tags = ["care", "hand-cream", "luxury"],
                Rating = 4.8,
                ReviewCount = 144,
                Weight = 0.12,
                Images = []
            };

            // ── Holiday products ───────────────────────────────────────────────

            var prodHoliday = new Product
            {
                NodeID = nodeHoliday.NodeID,
                DocumentName = "Limited Edition Holiday Bundle",

                ProductName = "Limited Edition Holiday Bundle",
                ProductType = ProductType.BUNDLE,
                ProductGroup = ProductGroup.SORT_GOODS,
                ShortDescription = "Six festive shades that capture the warmth and sparkle of the holidays.",
                Description = "<p>Celebrate the season with this curated holiday collection. Six festive shades that capture the warmth and sparkle of the holidays — cranberry, champagne gold, pine green, ruby red, snow white, and midnight blue. Presented in a beautiful gift box.</p>",
                Price = 107m,
                PriceDiscount = 0m,
                IsNew = true,
                IsBestSeller = false,
                Tags = ["bundle", "holiday", "limited"],
                Rating = 4.9,
                ReviewCount = 87,
                Weight = 0.5,
                IsNeedBox = true,
                Images = []
            };

            await context.Products.AddRangeAsync(
                prodBlackWhite, prodNude, prodNeon, prodTopCoat,
                prodChrome,
                prodHalloween, prodStarterKit,
                prodBrushSet, prodFileSet,
                prodCuticle, prodHandCream,
                prodHoliday
            );
            await context.SaveChangesAsync();

            // ── Product Variants ───────────────────────────────────────────────
            //
            // MOCK_COLORS palette from lum-nails ProductInfo component:
            //   Blush #E8B4B8 | Champagne #D4AF7E | Mint #A8D5B5 | Steel Blue #6B90C7
            //   Orchid #C49FD4 | Rose #E8407A | Crimson #B03A2E | Burgundy #6D2B3D | Charcoal #4A4A4A
            //
            // Each product gets 1–3 variants with relevant colors from the palette.

            var variants = new List<ProductVariant>
            {
                // ── Black + White Gel Duo ──────────────────────────────────────
                new() {
                    ProductID = prodBlackWhite.PageID, SKU = "GLB-BW-BLK",
                    UPC = "810000000031", VariantName = "Black",
                    Color = "Black",    ColorHex = "#1A1A1A",  Stock = 25, Images = []
                },
                new() {
                    ProductID = prodBlackWhite.PageID, SKU = "GLB-BW-WHT",
                    UPC = "810000000032", VariantName = "White",
                    Color = "White",    ColorHex = "#F5F5F5",  Stock = 25, Images = []
                },

                // ── Nude Collection 6pc ───────────────────────────────────────
                new() {
                    ProductID = prodNude.PageID, SKU = "GEL-NUDE6-BLS",
                    UPC = "810000000051", VariantName = "Blush",
                    Color = "Blush",    ColorHex = "#E8B4B8",  Stock = 15, Images = []
                },
                new() {
                    ProductID = prodNude.PageID, SKU = "GEL-NUDE6-CHP",
                    UPC = "810000000052", VariantName = "Champagne",
                    Color = "Champagne",ColorHex = "#D4AF7E",  Stock = 12, Images = []
                },
                new() {
                    ProductID = prodNude.PageID, SKU = "GEL-NUDE6-ROS",
                    UPC = "810000000053", VariantName = "Rose",
                    Color = "Rose",     ColorHex = "#E8407A",  Stock = 8,  Images = []
                },

                // ── Neon Summer 4pc ───────────────────────────────────────────
                new() {
                    ProductID = prodNeon.PageID, SKU = "GEL-NEON4-MNT",
                    UPC = "810000000081", VariantName = "Mint Green",
                    Color = "Mint",     ColorHex = "#A8D5B5",  Stock = 10, Images = []
                },
                new() {
                    ProductID = prodNeon.PageID, SKU = "GEL-NEON4-ORD",
                    UPC = "810000000082", VariantName = "Orchid",
                    Color = "Orchid",   ColorHex = "#C49FD4",  Stock = 10, Images = []
                },
                new() {
                    ProductID = prodNeon.PageID, SKU = "GEL-NEON4-SBL",
                    UPC = "810000000083", VariantName = "Steel Blue",
                    Color = "Steel Blue",ColorHex = "#6B90C7", Stock = 8,  Images = []
                },

                // ── Ultra-Shine Top Coat ──────────────────────────────────────
                new() {
                    ProductID = prodTopCoat.PageID, SKU = "GEL-TOP-15",
                    UPC = "810000000101", VariantName = "15ml Bottle",
                    Stock = 40, Images = []
                },
                new() {
                    ProductID = prodTopCoat.PageID, SKU = "GEL-TOP-30",
                    UPC = "810000000102", VariantName = "30ml Bottle",
                    Stock = 30, Images = []
                },

                // ── Chrome Powder Kit ─────────────────────────────────────────
                new() {
                    ProductID = prodChrome.PageID, SKU = "ART-CHROME-GLD",
                    UPC = "810000000111", VariantName = "Gold",
                    Color = "Gold",     ColorHex = "#D4AF7E",  Stock = 12, Images = []
                },
                new() {
                    ProductID = prodChrome.PageID, SKU = "ART-CHROME-SLV",
                    UPC = "810000000112", VariantName = "Silver",
                    Color = "Silver",   ColorHex = "#C0C0C0",  Stock = 12, Images = []
                },
                new() {
                    ProductID = prodChrome.PageID, SKU = "ART-CHROME-RSG",
                    UPC = "810000000113", VariantName = "Rose Gold",
                    Color = "Rose Gold",ColorHex = "#E8B4B8",  Stock = 9,  Images = []
                },

                // ── V.2 Halloween Bundle ──────────────────────────────────────
                new() {
                    ProductID = prodHalloween.PageID, SKU = "GLB-V2",
                    UPC = "810000000011", VariantName = "Halloween Bundle V.2",
                    Stock = 23, Images = []
                },

                // ── Complete Gel Starter Kit ──────────────────────────────────
                new() {
                    ProductID = prodStarterKit.PageID, SKU = "KIT-START",
                    UPC = "810000000071", VariantName = "Complete Starter Kit",
                    Stock = 18, Images = []
                },

                // ── Precision Brush Set ───────────────────────────────────────
                new() {
                    ProductID = prodBrushSet.PageID, SKU = "NAB-PRO",
                    UPC = "810000000041", VariantName = "8pc Brush Set",
                    Stock = 40, Images = []
                },

                // ── Nail File & Buffer Set ────────────────────────────────────
                new() {
                    ProductID = prodFileSet.PageID, SKU = "TOOL-FILE",
                    UPC = "810000000091", VariantName = "5pc File & Buffer Set",
                    Stock = 80, Images = []
                },

                // ── Cuticle Oil Trio (3 scents) ───────────────────────────────
                new() {
                    ProductID = prodCuticle.PageID, SKU = "CARE-OIL3-LAV",
                    UPC = "810000000061", VariantName = "Lavender",
                    Color = "Lavender", ColorHex = "#C49FD4",  Stock = 25, Images = []
                },
                new() {
                    ProductID = prodCuticle.PageID, SKU = "CARE-OIL3-CIT",
                    UPC = "810000000062", VariantName = "Citrus",
                    Color = "Citrus",   ColorHex = "#D4AF7E",  Stock = 20, Images = []
                },
                new() {
                    ProductID = prodCuticle.PageID, SKU = "CARE-OIL3-UNS",
                    UPC = "810000000063", VariantName = "Unscented",
                    Stock = 15, Images = []
                },

                // ── Luxury Hand Cream (2 sizes) ───────────────────────────────
                new() {
                    ProductID = prodHandCream.PageID, SKU = "CARE-HC-75",
                    UPC = "810000000121", VariantName = "75ml",
                    Stock = 30, Images = []
                },
                new() {
                    ProductID = prodHandCream.PageID, SKU = "CARE-HC-150",
                    UPC = "810000000122", VariantName = "150ml",
                    Stock = 15, Images = []
                },

                // ── Limited Edition Holiday Bundle ────────────────────────────
                new() {
                    ProductID = prodHoliday.PageID, SKU = "GLB-HV2",
                    UPC = "810000000021", VariantName = "Holiday Bundle",
                    Stock = 15, Images = []
                }
            };

            await context.ProductVariants.AddRangeAsync(variants);
            await context.SaveChangesAsync();
        }

        // ─── Closure Table Builder ────────────────────────────────────────────────

        /// <summary>
        /// Builds the full closure table (DocumentLinkedNode) for the entire tree.
        /// Each node emits: (self,self,0), (parent,self,1), (grandparent,self,2), …
        /// </summary>
        private static IEnumerable<DocumentLinkedNode> BuildClosureTable(
            (int NodeID, int? ParentNodeID)[] nodes)
        {
            var parentMap = nodes.ToDictionary(n => n.NodeID, n => n.ParentNodeID);

            foreach (var (nodeId, _) in nodes)
            {
                yield return new DocumentLinkedNode { Ancestor = nodeId, Descendant = nodeId, Depth = 0 };

                int depth = 1;
                int? current = parentMap[nodeId];
                while (current.HasValue)
                {
                    yield return new DocumentLinkedNode { Ancestor = current.Value, Descendant = nodeId, Depth = depth };
                    depth++;
                    current = parentMap.TryGetValue(current.Value, out var p) ? p : null;
                }
            }
        }
    }
}
