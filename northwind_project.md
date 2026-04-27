# Northwind Console Application Project

Using the Northwind database created in a previous lesson, you will create a console application with a menu driven UI that allows maintenance of selected data tables. The minimum requirements for this project are as follows:

---

## Grading Requirements

### Grade: C (405 / 500 points)

Your application must perform the following on demand:

1. Add new records to the **Products** table
2. Edit a specified record from the **Products** table
3. Display all records in the **Products** table (`ProductName` only) — user decides if they want to see:
   - All products
   - Discontinued products
   - Active (not discontinued) products

   > Discontinued products should be distinguished from active products.

4. Display a specific Product (all product fields should be displayed)
5. Use **NLog** to track user functions

---

### Grade: B (445 / 500 points)

Your application must include all **C** features, plus:

1. Add new records to the **Categories** table
2. Edit a specified record from the **Categories** table
3. Display all Categories in the **Categories** table (`CategoryName` and `Description`)
4. Display all Categories and their related **active** (not discontinued) product data (`CategoryName`, `ProductName`)
5. Display a specific Category and its related active product data (`CategoryName`, `ProductName`)

---

### Grade: A (475 / 500 points)

Your application must include all **B** & **C** features, plus:

1. Delete a specified existing record from the **Products** table *(account for orphans in related tables)*
2. Delete a specified existing record from the **Categories** table *(account for orphans in related tables)*
3. Use **data annotations** and handle **all user errors gracefully** & log all errors using **NLog**

---

### Full Credit (500 / 500 points)

Your application must do something **exceptional** — something not covered in class.

> Don't ask the instructor what it should do; it's your job to figure it out!

---

## Submission Requirements

- Your instructor will provide **extremely limited support** on this project.
- The **C** & **B** level work has all been demonstrated in class, as well as most of the **A** level work. Utilize the in-class examples when you need assistance.
- The full functionality of your project **must be demonstrated with a video**.
- A **link to the video** must be submitted along with a link to your **GitHub repo**.
- Any feature **not demonstrated in the video** will not be applied to the project grade.
- At the **start of your video**, please announce which grade level you attempted.
