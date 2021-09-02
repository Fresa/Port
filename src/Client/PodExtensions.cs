using System.Collections.Generic;
using System.Linq;
using Port.Shared;

namespace Port.Client
{
    internal static class PodExtensions
    {
        internal static IEnumerable<Pod> WhereServiceIsHosted(
            this IEnumerable<Pod> pods,
            Service service)
        {
            return pods.Where(
                pod =>
                    service.Selectors.Any() &&
                    service.Selectors.All(
                        selector =>
                            pod.Labels.Any(
                                label =>
                                    label.Key == selector.Key &&
                                    label.Value == selector.Value)));
        }
    }
}