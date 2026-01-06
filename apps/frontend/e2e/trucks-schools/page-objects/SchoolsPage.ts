import { Page, Locator } from '@playwright/test';

/**
 * Page Object Model for Schools Management Page
 *
 * Encapsulates all interactions with the Schools CRUD interface
 * for cleaner, more maintainable tests.
 */
export class SchoolsPage {
  readonly page: Page;
  readonly schoolsTable: Locator;
  readonly addButton: Locator;
  readonly tableRows: Locator;

  // Form locators
  readonly schoolForm: Locator;
  readonly nameInput: Locator;
  readonly shortNameInput: Locator;
  readonly contactPersonInput: Locator;
  readonly contactCellInput: Locator;
  readonly emailInput: Locator;
  readonly addressInput: Locator;

  // Billing form locators
  readonly priceInput: Locator;
  readonly feeDescriptionInput: Locator;
  readonly formulaInput: Locator;

  readonly submitButton: Locator;
  readonly cancelButton: Locator;

  // Validation locators
  readonly validationErrors: Locator;

  constructor(page: Page) {
    this.page = page;

    // Table and list elements
    this.schoolsTable = page.locator('table');
    this.tableRows = page.locator('table tbody tr');
    this.addButton = page.locator('button').filter({ hasText: /add|new|create/i }).first();

    // Form elements
    this.schoolForm = page.locator('form');
    this.nameInput = this.schoolForm.locator('input[name="name"], input[placeholder*="name" i], #name').first();
    this.shortNameInput = this.schoolForm.locator('input[name="shortName"]').first();
    this.contactPersonInput = this.schoolForm.locator('input[name="contactPerson"], input[placeholder*="contact person" i]').first();
    this.contactCellInput = this.schoolForm.locator('input[name="contactCell"], input[placeholder*="cell" i], input[placeholder*="mobile" i]').first();
    this.emailInput = this.schoolForm.locator('input[name="email"], input[type="email"]').first();
    this.addressInput = this.schoolForm.locator('input[name="address"], textarea[name="address"], input[placeholder*="address" i]').first();

    // Billing fields
    this.priceInput = this.schoolForm.locator('input[name="price"], input[name="rate"], input[placeholder*="price" i], input[placeholder*="rate" i]').first();
    this.feeDescriptionInput = this.schoolForm.locator('input[name="feeDescription"], textarea[name="feeDescription"]').first();
    this.formulaInput = this.schoolForm.locator('input[name="formula"]').first();

    // Buttons
    this.submitButton = this.schoolForm.locator('button[type="submit"]');
    this.cancelButton = this.schoolForm.locator('button').filter({ hasText: /cancel|back/i }).first();

    // Validation
    this.validationErrors = page.locator('text=/required|mandatory|invalid/i');
  }

  /**
   * Navigate to the schools page
   */
  async goto() {
    await this.page.goto('/schools');
    await this.page.waitForLoadState('networkidle');
    await this.page.waitForTimeout(1000);
  }

  /**
   * Get the number of schools in the list
   */
  async getSchoolCount(): Promise<number> {
    return await this.tableRows.count();
  }

  /**
   * Find a school row by name
   */
  findSchoolByName(name: string): Locator {
    return this.tableRows.filter({ hasText: name }).first();
  }

  /**
   * Click add button to open create form
   */
  async clickAddSchool() {
    await this.addButton.click();
    await this.page.waitForLoadState('networkidle');
    await this.page.waitForTimeout(500);
  }

  /**
   * Click edit button for a specific school
   */
  async clickEditSchool(schoolName: string) {
    const schoolRow = this.findSchoolByName(schoolName);
    const editButton = schoolRow.locator('button').filter({ hasText: /edit|modify/i }).first();

    if (await editButton.count() > 0) {
      await editButton.click();
    } else {
      await schoolRow.click();
    }

    await this.page.waitForLoadState('networkidle');
    await this.page.waitForTimeout(500);
  }

  /**
   * Click delete/archive button for a specific school
   */
  async clickDeleteSchool(schoolName: string) {
    const schoolRow = this.findSchoolByName(schoolName);
    const deleteButton = schoolRow.locator('button').filter({ hasText: /delete|remove|archive/i }).first();
    const archiveButton = schoolRow.locator('button').filter({ hasText: /archive|deactivate/i }).first();

    // Prefer archive button, fall back to delete
    const buttonToClick = (await archiveButton.count() > 0) ? archiveButton : deleteButton;

    // Set up dialog handler before clicking
    this.page.on('dialog', async dialog => {
      await dialog.accept();
    });

    await buttonToClick.click();
    await this.page.waitForLoadState('networkidle');
  }

  /**
   * Fill school form with provided data
   */
  async fillSchoolForm(data: {
    name: string;
    shortName?: string;
    contactPerson?: string;
    contactCell?: string;
    email?: string;
    address?: string;
  }) {
    await this.nameInput.fill(data.name);

    if (data.shortName && await this.shortNameInput.isVisible()) {
      await this.shortNameInput.fill(data.shortName);
    }

    if (data.contactPerson && await this.contactPersonInput.isVisible()) {
      await this.contactPersonInput.fill(data.contactPerson);
    }

    if (data.contactCell && await this.contactCellInput.isVisible()) {
      await this.contactCellInput.fill(data.contactCell);
    }

    if (data.email && await this.emailInput.isVisible()) {
      await this.emailInput.fill(data.email);
    }

    if (data.address && await this.addressInput.isVisible()) {
      await this.addressInput.fill(data.address);
    }
  }

  /**
   * Fill billing settings form
   */
  async fillBillingSettings(data: {
    price?: string;
    feeDescription?: string;
    formula?: string;
  }) {
    if (data.price && await this.priceInput.isVisible()) {
      await this.priceInput.fill(data.price);
    }

    if (data.feeDescription && await this.feeDescriptionInput.isVisible()) {
      await this.feeDescriptionInput.fill(data.feeDescription);
    }

    if (data.formula && await this.formulaInput.isVisible()) {
      await this.formulaInput.fill(data.formula);
    }
  }

  /**
   * Submit the school form
   */
  async submitForm() {
    await this.submitButton.click();
    await this.page.waitForURL(/\/schools/, { timeout: 10000 });
    await this.page.waitForLoadState('networkidle');
    await this.page.waitForTimeout(1000);
  }

  /**
   * Cancel form without saving
   */
  async cancelForm() {
    if (await this.cancelButton.count() > 0) {
      await this.cancelButton.click();
      await this.page.waitForURL(/\/schools$/, { timeout: 5000 });
    }
  }

  /**
   * Check if school is visible in list
   */
  async isSchoolVisible(schoolName: string): Promise<boolean> {
    const schoolLocator = this.page.locator(`text=${schoolName}`);
    return await schoolLocator.count() > 0;
  }

  /**
   * Search for schools by text
   */
  async search(searchTerm: string) {
    const searchInput = this.page.locator('input[placeholder*="search" i], input[type="search"], #search');

    if (await searchInput.count() > 0) {
      await searchInput.first().fill(searchTerm);
      await this.page.waitForTimeout(1000);
    }
  }

  /**
   * Clear search
   */
  async clearSearch() {
    const searchInput = this.page.locator('input[placeholder*="search" i], input[type="search"], #search');

    if (await searchInput.count() > 0) {
      await searchInput.first().clear();
      await this.page.waitForTimeout(1000);
    }
  }

  /**
   * Get validation error text
   */
  async getValidationErrors(): Promise<string[]> {
    const errors: string[] = [];
    const count = await this.validationErrors.count();

    for (let i = 0; i < count; i++) {
      const text = await this.validationErrors.nth(i).textContent();
      if (text) errors.push(text);
    }

    return errors;
  }

  /**
   * Get current value of a field
   */
  async getFieldValue(fieldName: 'name' | 'contactPerson' | 'contactCell' | 'email' | 'price' | 'feeDescription'): Promise<string> {
    let input: Locator;

    switch (fieldName) {
      case 'name':
        input = this.nameInput;
        break;
      case 'contactPerson':
        input = this.contactPersonInput;
        break;
      case 'contactCell':
        input = this.contactCellInput;
        break;
      case 'email':
        input = this.emailInput;
        break;
      case 'price':
        input = this.priceInput;
        break;
      case 'feeDescription':
        input = this.feeDescriptionInput;
        break;
      default:
        throw new Error(`Unknown field name: ${fieldName}`);
    }

    return await input.inputValue();
  }
}
