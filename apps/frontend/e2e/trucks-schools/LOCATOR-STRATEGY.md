# Locator Strategy for Trucks & Schools E2E Tests

## Overview

This document explains the locator strategy used in the E2E tests to ensure reliable, maintainable test automation.

## Priority Order for Locators

When writing or refactoring tests, use locators in this order of preference:

### 1. **Best: By ID or data-testid** (Most Stable)
```typescript
// ✅ PREFERRED - Unique ID, won't change with UI updates
page.locator('#email')
page.locator('[data-testid="submit-button"]')
```

### 2. **By Name Attribute** (Very Stable)
```typescript
// ✅ GOOD - Form field names are stable
page.locator('input[name="email"]')
page.locator('textarea[name="notes"]')
```

### 3. **By ARIA Label** (Accessible & Stable)
```typescript
// ✅ GOOD - Accessible labels rarely change
page.locator('button[aria-label="Close"]')
page.locator('table[aria-label="Trucks Registry"]')
```

### 4. **By Role** (Semantic & Stable)
```typescript
// ✅ GOOD - Semantic HTML roles
page.locator('button[type="submit"]')
page.locator('input[type="email"]')
```

### 5. **By Text Content** (Use Carefully)
```typescript
// ⚠️ ACCEPTABLE - Only for static text that won't change
page.locator('text=Trucks') // Page heading
page.locator('text=/success|created/i') // Dynamic message pattern

// ❌ AVOID - Text content that changes
page.locator('text=Submit') // May be translated or changed
```

### 6. **By CSS Class** (Fragile - Last Resort)
```typescript
// ⚠️ FRAGILE - CSS classes can change with styling updates
page.locator('.btn-primary')
page.locator('[class*="button"]')
```

## Page Object Model Implementation

The Page Object Model (POM) classes encapsulate locator logic:

### TrucksPage Example
```typescript
export class TrucksPage {
  readonly page: Page;
  readonly nameInput: Locator;
  readonly submitButton: Locator;

  constructor(page: Page) {
    this.page = page;

    // ✅ Uses specific locators, not fuzzy text
    this.nameInput = page.locator('input[name="name"]').first();
    this.submitButton = page.locator('button[type="submit"]');
  }

  async fillTruckForm(data: { name: string }) {
    // Encapsulated interaction with stable locators
    await this.nameInput.fill(data.name);
  }
}
```

### Benefits of POM
- **Centralized locator management** - Change locators in one place
- **Reusable actions** - Common operations defined once
- **Better abstraction** - Tests focus on business logic, not UI details
- **Easier maintenance** - UI changes only affect page objects

## Current Locator Usage in Tests

### Trucks Page (`trucks-crud.spec.ts`)

**Good Examples:**
- `page.locator('#email')` - ID selector ✅
- `page.locator('input[name="name"]')` - Name attribute ✅
- `page.locator('button[type="submit"]')` - Button role ✅
- `page.locator('table[aria-label="Trucks Registry"]')` - ARIA label ✅

**Needs Improvement:**
- `page.locator('text=Alpha')` - Text content, could use ID ⚠️
- `page.locator('text=/success|created/i')` - Acceptable for dynamic content ⚠️

### Schools Page (`schools-crud.spec.ts`)

**Good Examples:**
- `page.locator('input[name="contactPerson"]')` - Name attribute ✅
- `page.locator('input[type="email"]')` - Input type ✅
- `page.locator('h1').filter({ hasText: /Schools/i })` - Structural + text combo ✅

**Needs Improvement:**
- `page.locator('text=Uitstaande')` - Text content, could use data attribute ⚠️

## Recommendations for Improvement

### Add data-testid Attributes

Add test-specific attributes to UI elements for more reliable selectors:

```html
<!-- In Angular component templates -->
<button data-testid="truck-add-button">Add Truck</button>
<input data-testid="truck-name-input" name="name" />
<button data-testid="truck-submit-button" type="submit">Save</button>

<tr data-testid="truck-row-{{truck.id}}">
  <td data-testid="truck-name">{{truck.name}}</td>
  <td>
    <button data-testid="truck-edit-{{truck.id}}">Edit</button>
    <button data-testid="truck-delete-{{truck.id}}">Delete</button>
  </td>
</tr>
```

Then use in tests:
```typescript
page.locator('[data-testid="truck-add-button"]')
page.locator('[data-testid="truck-row-1"]')
page.locator(`[data-testid="truck-edit-${truckId}"]`)
```

### Migrate Existing Tests to Page Objects

Gradually migrate test methods to use the Page Object classes:

**Before:**
```typescript
await page.goto('/trucks');
await page.locator('input[name="name"]').fill('Truck 1');
await page.locator('button[type="submit"]').click();
```

**After:**
```typescript
const trucksPage = new TrucksPage(page);
await trucksPage.goto();
await trucksPage.fillTruckForm({ name: 'Truck 1' });
await trucksPage.submitForm();
```

## Handling Dynamic Content

### Search Results with Dynamic IDs
```typescript
// ❌ BAD - Hardcoded index
await page.locator('table tbody tr').nth(0).click();

// ✅ GOOD - Filter by content
const truckRow = page.locator('table tbody tr').filter({ hasText: 'Alpha' });
await truckRow.click();

// ✅ BETTER - Use data-testid (if available)
await page.locator('[data-testid="truck-1"]').click();
```

### Dynamic Success Messages
```typescript
// ✅ GOOD - Pattern matching for dynamic content
const successMessage = page.locator('text=/success|created|added/i');
await expect(successMessage).toBeVisible();
```

## Anti-Patterns to Avoid

### ❌ Don't Use XPath Unless Necessary
```typescript
// ❌ Brittle and hard to read
page.locator('//div[@class="container"]/form/input[1]')

// ✅ Use user-facing locators
page.locator('input[name="email"]')
```

### ❌ Don't Chain Multiple Locators
```typescript
// ❌ Fragile chain
page.locator('div').locator('form').locator('input').nth(1)

// ✅ Direct, specific locator
page.locator('input[name="email"]')
```

### ❌ Don't Rely on CSS Classes
```typescript
// ❌ Will break if styling changes
page.locator('.btn.btn-primary')

// ✅ Use semantic attributes
page.locator('button[type="submit"]')
page.locator('[data-testid="submit-button"]')
```

### ❌ Don't Use Complex Text Matching
```typescript
// ❌ Too specific, will break with minor UI changes
page.locator('text=Click here to add a new truck to the fleet')

// ✅ Use meaningful attributes
page.locator('button[aria-label="Add Truck"]')
page.locator('[data-testid="add-truck-button"]')
```

## Best Practices Summary

1. **Prefer semantic attributes** (id, name, aria-label, role)
2. **Add data-testid** for test-specific elements
3. **Use Page Objects** to centralize locator logic
4. **Avoid UI-dependent selectors** (CSS classes, complex text)
5. **Use filter()** with text only for dynamic lists, not individual elements
6. **Document locator strategy** when introducing new patterns

## Tools for Finding Locators

Playwright provides tools to help you find the best locators:

```bash
# Run in headed mode with inspector
npx playwright test --debug

# Use Playwright Inspector
npx playwright codegen http://localhost:4200/trucks

# Test locators in console
await page.locator('your-selector').count()
```

## References

- [Playwright Locator Best Practices](https://playwright.dev/docs/best-practices#1-use-testids)
- [Playwright Locators](https://playwright.dev/docs/locators)
- [Page Object Model](https://playwright.dev/docs/pom)
