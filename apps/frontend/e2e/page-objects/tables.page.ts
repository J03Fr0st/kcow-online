import { type Page, type Locator } from '@playwright/test';
import { BasePage } from './base.page';

/**
 * Page object for Tables functionality
 */
export class TablesPage extends BasePage {
  readonly table: Locator;
  readonly tableRows: Locator;
  readonly tableHeaders: Locator;
  readonly searchInput: Locator;
  readonly filterButton: Locator;
  readonly sortButtons: Locator;
  readonly columnToggle: Locator;
  readonly saveViewButton: Locator;
  readonly loadViewButton: Locator;
  readonly pagination: Locator;
  readonly rowsPerPageSelect: Locator;

  constructor(page: Page) {
    super(page);

    this.table = page.locator('table').first();
    this.tableRows = this.table.locator('tbody tr');
    this.tableHeaders = this.table.locator('thead th');
    this.searchInput = page.locator('input[placeholder*="Search" i], input[type="search"]');
    this.filterButton = page.locator('button', { hasText: /filter/i });
    this.sortButtons = page.locator('th button, th [role="button"]');
    this.columnToggle = page.locator('button', { hasText: /column/i });
    this.saveViewButton = page.locator('button', { hasText: /save view/i });
    this.loadViewButton = page.locator('button', { hasText: /load view/i });
    this.pagination = page.locator('.pagination, [role="navigation"]');
    this.rowsPerPageSelect = page.locator('select').filter({ hasText: /rows|items/i });
  }

  async gotoTables(): Promise<void> {
    await this.goto('/tables');
  }

  async searchTable(query: string): Promise<void> {
    await this.searchInput.fill(query);
    await this.helpers.waitForAngular();
  }

  async sortByColumn(columnName: string): Promise<void> {
    const header = this.tableHeaders.filter({ hasText: columnName });
    await header.click();
    await this.helpers.waitForAngular();
  }

  async getRowCount(): Promise<number> {
    return await this.tableRows.count();
  }

  async getCellContent(row: number, column: number): Promise<string> {
    const cell = this.tableRows.nth(row).locator('td').nth(column);
    return (await cell.textContent()) || '';
  }

  async toggleColumn(columnName: string): Promise<void> {
    await this.columnToggle.click();
    await this.page.locator(`label, input`).filter({ hasText: columnName }).click();
    await this.helpers.waitForAngular();
  }

  async isColumnVisible(columnName: string): Promise<boolean> {
    const header = this.tableHeaders.filter({ hasText: columnName });
    return await header.isVisible();
  }

  async saveView(viewName: string): Promise<void> {
    await this.saveViewButton.click();
    await this.page.locator('input[placeholder*="name" i]').fill(viewName);
    await this.page.locator('button', { hasText: /save|ok/i }).click();
    await this.helpers.waitForAngular();
  }

  async loadView(viewName: string): Promise<void> {
    await this.loadViewButton.click();
    await this.page.locator('text=' + viewName).click();
    await this.helpers.waitForAngular();
  }

  async goToPage(pageNumber: number): Promise<void> {
    await this.pagination.locator(`button:has-text("${pageNumber}")`).click();
    await this.helpers.waitForAngular();
  }

  async setRowsPerPage(count: number): Promise<void> {
    await this.rowsPerPageSelect.selectOption(count.toString());
    await this.helpers.waitForAngular();
  }
}
