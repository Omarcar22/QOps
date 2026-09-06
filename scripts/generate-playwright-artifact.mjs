import fs from 'node:fs';
import path from 'node:path';

const root = process.cwd();
const resultsPath = path.join(root, 'playwright-report', 'results.json');
const artifactDir = path.join(root, 'artifacts', 'qa-evidence');
const outputPath = path.join(artifactDir, 'playwright-evidence.json');

if (!fs.existsSync(resultsPath)) {
  const fallback = {
    project: 'QOps',
    generatedAt: new Date().toISOString(),
    summary: {
      total: 1,
      passed: 0,
      failed: 1,
      planned: 0,
    },
    tests: [
      {
        name: 'Playwright smoke validation',
        suite: 'Playwright',
        status: 'Failed',
        duration: 'Pending',
        result: 'Playwright report not found; browser suite has not run yet.',
        executedAt: new Date().toISOString(),
      },
    ],
  };

  fs.mkdirSync(artifactDir, { recursive: true });
  fs.writeFileSync(outputPath, `${JSON.stringify(fallback, null, 2)}\n`, 'utf8');
  console.log(`Fallback artifact generated at ${outputPath}`);
  process.exit(0);
}

const results = JSON.parse(fs.readFileSync(resultsPath, 'utf8'));
const passed = results.stats?.passed ?? 0;
const failed = results.stats?.failed ?? 0;
const skipped = results.stats?.skipped ?? 0;
const total = results.stats?.total ?? 0;

const report = {
  project: 'QOps',
  generatedAt: new Date().toISOString(),
  summary: {
    total,
    passed,
    failed,
    planned: skipped,
  },
  tests: (results.suites ?? []).flatMap((suite) =>
    (suite.specs ?? []).flatMap((spec) =>
      (spec.tests ?? []).map((test) => ({
        name: test.title ?? spec.title,
        suite: suite.title,
        status: test.results?.[0]?.status === 'passed' ? 'Passed' : 'Failed',
        duration: `${(((test.results?.[0]?.duration ?? 0) / 1000) || 0).toFixed(1)}s`,
        result: test.results?.[0]?.status === 'passed'
          ? 'Playwright assertion passed'
          : (test.results?.[0]?.error?.message ?? 'Playwright assertion failed'),
        executedAt: new Date().toISOString(),
      })),
    ),
  ),
};

fs.mkdirSync(artifactDir, { recursive: true });
fs.writeFileSync(outputPath, `${JSON.stringify(report, null, 2)}\n`, 'utf8');
console.log(`Playwright artifact generated at ${outputPath}`);
