# ADR-0013: Use TDD for the domain and BDD for front-end features

- **Status:** Accepted
- **Date:** 2026-09-08
- **Deciders:** NahuelFernandezGalli
- **Supersedes:** [ADR-0010](0010-use-tdd-for-domain-and-behavior-tests-for-ui.md)

## Context

[ADR-0010](0010-use-tdd-for-domain-and-behavior-tests-for-ui.md) asked for strict TDD in the
domain and for behavior tests in the front end written "once a screen takes shape". That second
half leaves the order of work open, and in practice it lets a screen be built first and described
afterwards, which produces tests that mirror the components that happen to exist instead of the
behavior the user needs.

Buckl's front end is where the product is decided: what a wardrobe shows when it is empty, what
happens when a photo fails to upload, what the user can and cannot edit on an archived garment.
Those are the questions worth settling before writing a component, and they are worth writing down
in language that does not mention React.

## Decision

Every front-end feature follows a full behavior-driven development cycle, in this order:

1. **Formulate the scenario.** Write it in Gherkin in a `.feature` file, in user language, before
   touching component code.
2. **Automate it.** Bind each step to code and run it. The scenario must fail for the right
   reason.
3. **Implement.** Write the minimum needed to make the scenario pass.
4. **Refactor.** Improve the code with the scenario green.

Scenarios are Gherkin `.feature` files executed by Vitest through
[`@amiceli/vitest-cucumber`](https://www.npmjs.com/package/@amiceli/vitest-cucumber), with
Testing Library driving the rendered components. No second test runner and no extra CI job: the
existing `npm run test` and the `Web` check cover them.

The domain and application layers in .NET keep strict test-driven development with xUnit, exactly
as ADR-0010 stated. Pure front-end helpers with no user-visible behavior of their own, such as
image compression or currency formatting, keep plain unit tests.

The concrete workflow, file layout and scenario-writing rules live in
[the testing strategy](../testing.md).

## Alternatives considered

- **Keep ADR-0010 as written** — behavior tests with no prescribed order. Cheapest, but it does
  not give the front end the design pressure that TDD gives the domain, which is the gap this
  decision closes.
- **Given/When/Then structure inside the test files, without `.feature` files** — no new
  dependency and nothing to keep in sync, but the scenario is not an artifact of its own, so
  writing it first stays a habit rather than a step. Rejected because the point of the decision is
  to make the cycle structural.
- **Cucumber.js with its own runner, plus Playwright for end-to-end scenarios** — the most
  faithful to classic BDD, but it adds a second runner, a separate CI job, and slow browser tests
  for an app that will spend phase 3 running against mock data. Reconsider for a small end-to-end
  suite once the API exists.

## Consequences

### Positive

- The behavior of a screen is agreed in writing, in plain language, before any component exists.
- Scenarios read as documentation of what the app does, and they are reviewable by someone who
  does not read React.
- `vitest-cucumber` fails the run when a scenario in a `.feature` file has no implementation or a
  step is missing, so the cycle cannot be skipped silently.
- No second runner, no new CI check, no change to the required checks on `main`.

### Negative

- Two files per feature, the `.feature` and its steps, that must stay in sync.
- One new development dependency, on a major version released recently, whose maintenance we
  inherit. If it stalls, the scenarios remain valid Gherkin and can move to another runner.
- Writing good scenarios is a skill; bad ones describe clicks instead of outcomes and become
  brittle. The rules in [the testing strategy](../testing.md) exist to counter that.
