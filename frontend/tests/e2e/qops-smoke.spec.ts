import { expect, test } from '@playwright/test'

test('QOps smoke flow authenticates a viewer and enforces write permissions', async ({ page }) => {
  const email = `qa.${Date.now()}@example.com`
  const password = 'QOpsSmoke123!'

  await page.goto('/')

  await page.getByRole('button', { name: 'Need an account? Register' }).click()
  await page.getByLabel('Email').fill(email)
  await page.getByLabel('Password').fill(password)
  await page.getByRole('button', { name: 'Create account' }).click()

  await expect(page.getByRole('heading', { name: 'Welcome back' })).toBeVisible()
  await page.getByLabel('Email').fill(email)
  await page.getByLabel('Password').fill(password)
  await expect(page.getByLabel('Password')).toHaveValue(password)
  await page.getByRole('button', { name: 'Sign in' }).click()

  await expect(page.getByRole('heading', { name: 'Projects' })).toBeVisible()
  await expect(page.getByRole('button', { name: 'Create project' })).toHaveCount(0)
  await expect(page.getByRole('button', { name: 'Create environment' })).toHaveCount(0)
  await expect(page.getByRole('button', { name: 'Create release' })).toHaveCount(0)
  await expect(page.getByRole('button', { name: 'Create deployment' })).toHaveCount(0)
})

test('QOps admin flow creates a project', async ({ page }) => {
  const email = process.env.E2E_ADMIN_EMAIL
  const password = process.env.E2E_ADMIN_PASSWORD

  test.skip(!email || !password, 'Requires E2E_ADMIN_EMAIL and E2E_ADMIN_PASSWORD')

  const name = `Admin project ${Date.now()}`
  await page.goto('/')
  await page.getByLabel('Email').fill(email!)
  await page.getByLabel('Password').fill(password!)
  await page.getByRole('button', { name: 'Sign in' }).click()

  await expect(page.getByRole('heading', { name: 'Projects' })).toBeVisible()
  await page.getByLabel('Name').fill(name)
  await page.getByLabel('Description').fill('Playwright admin validation project')
  await page.getByLabel('Environment').fill('Development')
  await page.getByLabel('Version').fill('1.0.0')
  await page.getByRole('button', { name: 'Create project' }).click()

  await expect(page.getByText(name)).toBeVisible({ timeout: 20_000 })
})

test('QOps logout does not leak admin panels or pipeline write controls', async ({ page }) => {
  const adminEmail = process.env.E2E_ADMIN_EMAIL
  const adminPassword = process.env.E2E_ADMIN_PASSWORD

  test.skip(!adminEmail || !adminPassword, 'Requires E2E_ADMIN_EMAIL and E2E_ADMIN_PASSWORD')

  const viewerEmail = `qa.viewer.${Date.now()}@example.com`
  const viewerPassword = 'QOpsViewer123!'
  const projectName = `Role isolation project ${Date.now()}`

  await page.goto('/')
  await page.getByLabel('Email').fill(adminEmail!)
  await page.getByLabel('Password').fill(adminPassword!)
  await page.getByRole('button', { name: 'Sign in' }).click()
  await expect(page.getByRole('heading', { name: 'Projects' })).toBeVisible()

  await page.getByLabel('Name').fill(projectName)
  await page.getByLabel('Description').fill('Role isolation validation project')
  await page.getByLabel('Environment').fill('Development')
  await page.getByLabel('Version').fill('1.0.0')
  await page.getByRole('button', { name: 'Create project' }).click()
  await expect(page.getByText(projectName)).toBeVisible({ timeout: 20_000 })

  await page.getByRole('button', { name: 'Users' }).click()
  await expect(page.getByRole('heading', { name: 'Users' })).toBeVisible()

  await page.getByLabel('Section').selectOption('projects')
  await page.getByRole('button', { name: 'Sign out' }).click()
  await page.getByRole('button', { name: 'Need an account? Register' }).click()
  await page.getByLabel('Email').fill(viewerEmail)
  await page.getByLabel('Password').fill(viewerPassword)
  await page.getByRole('button', { name: 'Create account' }).click()
  await expect(page.getByRole('heading', { name: 'Welcome back' })).toBeVisible()
  await page.getByLabel('Email').fill(viewerEmail)
  await page.getByLabel('Password').fill(viewerPassword)
  await page.getByRole('button', { name: 'Sign in' }).click()

  await expect(page.getByRole('heading', { name: 'Projects' })).toBeVisible()
  await expect(page.getByRole('button', { name: 'Users' })).toHaveCount(0)
  await expect(page.getByRole('heading', { name: 'Users' })).toHaveCount(0)

  const projectRow = page.getByRole('row').filter({ hasText: projectName })
  await projectRow.getByRole('button', { name: 'Pipelines' }).click()
  await expect(page.getByRole('button', { name: 'Add step' })).toHaveCount(0)
  await expect(page.getByRole('button', { name: 'Create pipeline' })).toHaveCount(0)
})

test('QOps dashboard remains usable on mobile', async ({ page }) => {
  const adminEmail = process.env.E2E_ADMIN_EMAIL
  const adminPassword = process.env.E2E_ADMIN_PASSWORD

  test.skip(!adminEmail || !adminPassword, 'Requires E2E_ADMIN_EMAIL and E2E_ADMIN_PASSWORD')

  await page.setViewportSize({ width: 390, height: 844 })
  await page.goto('/')
  await page.getByLabel('Email').fill(adminEmail!)
  await page.getByLabel('Password').fill(adminPassword!)
  await page.getByRole('button', { name: 'Sign in' }).click()

  await expect(page.getByRole('heading', { name: 'Projects' })).toBeVisible()
  await expect(page.getByLabel('Section')).toBeVisible()
  await expect(page.locator('body')).toHaveJSProperty('scrollWidth', 390)
})