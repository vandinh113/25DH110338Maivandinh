
using System;
using System.Linq;
using System.Web.Mvc;
using System.Globalization;
using Warehouse_25DH110338.Models;

namespace Warehouse_25DH110338.Controllers
{
    public class ProductsController : Controller
    {
        private Warehouse_25DH110338Entities db =
            new Warehouse_25DH110338Entities();

        // ==========================================
        // CÂU 2 - DANH SÁCH SẢN PHẨM
        // ==========================================

        [HttpGet]
        public ActionResult Index()
        {
            var products = db.Products
                .OrderBy(p => p.ProductID)
                .ToList();

            return View(products);
        }

        // CÂU 2 - MỞ TRANG CHỈNH SỬA
        [HttpGet]
        public ActionResult Edit(int id)
        {
            var product = db.Products.Find(id);

            if (product == null)
                return HttpNotFound();

            return View(product);
        }

        // CÂU 2 - LƯU CHỈNH SỬA
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(
            int id,
            string ProductName,
            string Category,
            string Price)
        {
            var product = db.Products.Find(id);

            if (product == null)
                return HttpNotFound();

            string name = (ProductName ?? "").Trim();
            string category = (Category ?? "").Trim();

            decimal price;
            bool validPrice = decimal.TryParse(
                Price,
                NumberStyles.Number,
                CultureInfo.InvariantCulture,
                out price);

            if (string.IsNullOrWhiteSpace(name))
                ModelState.AddModelError(
                    "ProductName",
                    "Tên sản phẩm không được để trống.");

            if (string.IsNullOrWhiteSpace(category))
                ModelState.AddModelError(
                    "Category",
                    "Danh mục không được để trống.");

            if (!validPrice || price <= 0)
                ModelState.AddModelError(
                    "Price",
                    "Đơn giá phải là số lớn hơn 0.");

            if (!ModelState.IsValid)
            {
                ViewBag.InputName = ProductName;
                ViewBag.InputCategory = Category;
                ViewBag.InputPrice = Price;

                return View(product);
            }

            product.ProductName = name;
            product.Category = category;
            product.Price = price;

            db.SaveChanges();

            TempData["Success"] =
                "Cập nhật sản phẩm thành công!";

            return RedirectToAction("Index");
        }

        // ==========================================
        // CÂU 3 - TRA CỨU HÀNG HÓA
        // ==========================================

        [HttpGet]
        public ActionResult Search(
            string keyword,
            string status,
            string sort)
        {
            var products = db.Products.AsQueryable();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                string key = keyword.Trim().ToLower();

                products = products.Where(p =>
                    p.ProductName.ToLower().Contains(key));
            }

            if (status == "het")
            {
                products = products.Where(p =>
                    p.Quantity == 0);
            }
            else if (status == "saphet")
            {
                products = products.Where(p =>
                    p.Quantity >= 1 && p.Quantity <= 4);
            }
            else if (status == "du")
            {
                products = products.Where(p =>
                    p.Quantity >= 5);
            }

            switch (sort)
            {
                case "price_asc":
                    products = products.OrderBy(p => p.Price);
                    break;

                case "price_desc":
                    products = products.OrderByDescending(
                        p => p.Price);
                    break;

                default:
                    products = products.OrderBy(
                        p => p.ProductID);
                    break;
            }

            ViewBag.Keyword = keyword;
            ViewBag.Status = status;
            ViewBag.Sort = sort;

            return View(products.ToList());
        }

        // ==========================================
        // CÂU 4 - KIỂM KÊ MỘT SẢN PHẨM
        // ==========================================

        [HttpGet]
        public ActionResult Inventory(int id)
        {
            var product = db.Products.Find(id);

            if (product == null)
                return HttpNotFound();

            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Inventory(
            int id,
            string actualQuantity)
        {
            var product = db.Products.Find(id);

            if (product == null)
                return HttpNotFound();

            int newQuantity;

            bool isValid = int.TryParse(
                actualQuantity, out newQuantity);

            if (!isValid || newQuantity < 0)
            {
                ModelState.AddModelError(
                    "actualQuantity",
                    "Số lượng kiểm kê phải là số nguyên không âm.");

                ViewBag.ActualQuantity = actualQuantity;

                return View(product);
            }

            int oldQuantity = product.Quantity;
            int difference = newQuantity - oldQuantity;

            product.Quantity = newQuantity;

            db.SaveChanges();

            string message;

            if (difference > 0)
            {
                message = "Chênh lệch tăng " +
                    difference + " sản phẩm.";
            }
            else if (difference < 0)
            {
                message = "Chênh lệch giảm " +
                    Math.Abs(difference) + " sản phẩm.";
            }
            else
            {
                message = "Không chênh lệch.";
            }

            TempData["InventoryMessage"] = message;
            TempData["OldQuantity"] = oldQuantity;
            TempData["NewQuantity"] = newQuantity;

            return RedirectToAction(
                "InventoryResult",
                new { id = id });
        }

        // CÂU 4 - KẾT QUẢ KIỂM KÊ
        [HttpGet]
        public ActionResult InventoryResult(int id)
        {
            var product = db.Products.Find(id);

            if (product == null)
                return HttpNotFound();

            if (TempData["InventoryMessage"] == null)
                return RedirectToAction("Index");

            ViewBag.Message =
                TempData["InventoryMessage"];

            ViewBag.OldQuantity =
                TempData["OldQuantity"];

            ViewBag.NewQuantity =
                TempData["NewQuantity"];

            return View(product);
        }

        // ==========================================
        // CÂU 5 - KIỂM KÊ ĐỒNG THỜI HAI SẢN PHẨM
        // ==========================================

        [HttpGet]
        public ActionResult InventoryTwo()
        {
            ViewBag.Products = db.Products
                .OrderBy(p => p.ProductID)
                .ToList();

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult InventoryTwo(
            int? productId1,
            int? productId2,
            int? quantity1,
            int? quantity2)
        {
            ViewBag.Products = db.Products
                .OrderBy(p => p.ProductID)
                .ToList();

            // Kiểm tra chọn đủ hai sản phẩm
            if (!productId1.HasValue ||
                !productId2.HasValue)
            {
                ViewBag.Error =
                    "Vui lòng chọn đủ hai sản phẩm.";

                return View();
            }

            // Không cho phép trùng ProductID
            if (productId1.Value == productId2.Value)
            {
                ViewBag.Error =
                    "Hai sản phẩm phải có ProductID khác nhau.";

                return View();
            }

            // Kiểm tra số lượng
            if (!quantity1.HasValue ||
                !quantity2.HasValue ||
                quantity1.Value < 0 ||
                quantity2.Value < 0)
            {
                ViewBag.Error =
                    "Số lượng kiểm kê phải là số nguyên không âm.";

                return View();
            }

            // Kiểm tra sản phẩm tồn tại
            var product1 =
                db.Products.Find(productId1.Value);

            var product2 =
                db.Products.Find(productId2.Value);

            if (product1 == null || product2 == null)
            {
                ViewBag.Error =
                    "Một hoặc cả hai sản phẩm không tồn tại.";

                return View();
            }

            int oldQuantity1 = product1.Quantity;
            int oldQuantity2 = product2.Quantity;

            // Transaction cập nhật đồng thời
            using (var transaction =
                db.Database.BeginTransaction())
            {
                try
                {
                    product1.Quantity = quantity1.Value;
                    product2.Quantity = quantity2.Value;

                    db.SaveChanges();

                    transaction.Commit();
                }
                catch (Exception)
                {
                    transaction.Rollback();

                    ViewBag.Error =
                        "Giao dịch thất bại. Không có sản phẩm nào được cập nhật.";

                    return View();
                }
            }

            ViewBag.Success =
                "Kiểm kê đồng thời hai sản phẩm thành công!";

            ViewBag.OldQuantity1 = oldQuantity1;
            ViewBag.OldQuantity2 = oldQuantity2;

            ViewBag.NewQuantity1 = quantity1.Value;
            ViewBag.NewQuantity2 = quantity2.Value;

            ViewBag.ProductName1 = product1.ProductName;
            ViewBag.ProductName2 = product2.ProductName;

            return View();
        }

        // ==========================================
        // CÂU 6 - THÊM SẢN PHẨM
        // KIỂM TRA TRÙNG TÊN
        // ==========================================

        // GET: Products/Create
        [HttpGet]
        public ActionResult Create()
        {
            return View();
        }

        // POST: Products/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(
            string ProductName,
            string Category,
            string Price,
            string Quantity)
        {
            string name = (ProductName ?? "").Trim();
            string category = (Category ?? "").Trim();

            decimal price;
            int quantity;

            bool validPrice = decimal.TryParse(
                Price,
                NumberStyles.Number,
                CultureInfo.InvariantCulture,
                out price);

            bool validQuantity = int.TryParse(
                Quantity, out quantity);

            // Kiểm tra tên sản phẩm
            if (string.IsNullOrWhiteSpace(name))
            {
                ModelState.AddModelError(
                    "ProductName",
                    "Tên sản phẩm không được để trống.");
            }

            // Kiểm tra danh mục
            if (string.IsNullOrWhiteSpace(category))
            {
                ModelState.AddModelError(
                    "Category",
                    "Danh mục không được để trống.");
            }

            // Kiểm tra giá
            if (!validPrice || price <= 0)
            {
                ModelState.AddModelError(
                    "Price",
                    "Đơn giá phải là số lớn hơn 0.");
            }

            // Kiểm tra số lượng
            if (!validQuantity || quantity < 0)
            {
                ModelState.AddModelError(
                    "Quantity",
                    "Số lượng phải là số nguyên không âm.");
            }

            // Kiểm tra trùng tên sản phẩm
            if (ModelState.IsValid)
            {
                var compareInfo =
                    new CultureInfo("vi-VN").CompareInfo;

                var options =
                    CompareOptions.IgnoreCase |
                    CompareOptions.IgnoreNonSpace;

                bool isDuplicate = db.Products
                    .Select(p => p.ProductName)
                    .ToList()
                    .Any(existing =>
                        compareInfo.Compare(
                            (existing ?? "").Trim(),
                            name,
                            options) == 0);

                if (isDuplicate)
                {
                    ModelState.AddModelError(
                        "ProductName",
                        "Tên sản phẩm đã tồn tại. Không thể thêm trùng tên.");
                }
            }

            // Nếu có lỗi, giữ lại dữ liệu đã nhập
            if (!ModelState.IsValid)
            {
                ViewBag.InputName = ProductName;
                ViewBag.InputCategory = Category;
                ViewBag.InputPrice = Price;
                ViewBag.InputQuantity = Quantity;

                return View();
            }

            // Tạo sản phẩm mới
            // ProductID do SQL Server tự tăng
            var product = new Product
            {
                ProductName = name,
                Category = category,
                Price = price,
                Quantity = quantity
            };

            // Lưu bằng Entity Framework
            db.Products.Add(product);

            db.SaveChanges();

            TempData["Success"] =
                "Thêm sản phẩm thành công!";

            return RedirectToAction("Index");
        }

        // ==========================================
        // GIẢI PHÓNG KẾT NỐI DATABASE
        // ==========================================

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }

            base.Dispose(disposing);
        }
        // ==========================================
        // CÂU 7 - THỐNG KÊ KẾT QUẢ KIỂM KÊ
        // ==========================================

        [HttpGet]
        public ActionResult Statistics()
        {
            // 1. Tổng số sản phẩm trong kho
            int totalProducts = db.Products.Count();

            // 2. Số sản phẩm hết hàng (Quantity = 0)
            int outOfStock = db.Products
                .Count(p => p.Quantity == 0);

            // 3. Số sản phẩm còn hàng
            int inStock = db.Products
                .Count(p => p.Quantity > 0);

            // 4. Tổng số lượng hàng tồn kho
            int totalQuantity = db.Products
                .Sum(p => (int?)p.Quantity) ?? 0;

            // 5. Tổng giá trị hàng tồn kho
            decimal totalInventoryValue = db.Products
                .Sum(p => (decimal?)(p.Price * p.Quantity)) ?? 0;

            // 6. Số sản phẩm sắp hết (1 đến 4)
            int lowStock = db.Products
                .Count(p => p.Quantity >= 1 && p.Quantity <= 4);

            // Truyền dữ liệu sang View
            ViewBag.TotalProducts = totalProducts;
            ViewBag.OutOfStock = outOfStock;
            ViewBag.InStock = inStock;
            ViewBag.TotalQuantity = totalQuantity;
            ViewBag.TotalInventoryValue = totalInventoryValue;
            ViewBag.LowStock = lowStock;

            // Lấy danh sách sản phẩm từ SQL Server
            var products = db.Products
                .OrderBy(p => p.ProductID)
                .ToList();

            return View(products);
        }

    }
}
