using Xunit.Sdk;
using Xunit.v3;

// Test parallelization is disabled on purpose. Catel's ServiceLocator and the static initializers of
// Catel-based types (models, view models, observable collections) are process-wide, and initializing them
// from several test threads at once can deadlock. The suite is small and fast, so serial execution is cheap.
[assembly: Parallelization(Mode = ParallelMode.None)]
