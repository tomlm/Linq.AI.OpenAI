﻿
using Iciclecreek.Async;
using OpenAI.Chat;

namespace Linq.AI.OpenAI
{

    public class ClassifiedItem<ItemT, CategoryT>
    {
        public CategoryT Category { get; set; } = default!;

        public ItemT Item { get; set; } = default!;
    }

    public static class ClassifyExtension
    {
        /// <summary>
        /// Classify source by enum using AI model
        /// </summary>
        /// <typeparam name="EnumT">enumeration to use as classification categories</typeparam>
        /// <param name="source">source to process</param>
        /// <param name="model">chat client to use for the model</param>
        /// <param name="instructions">(optional) extend instructions.</param>
        /// <param name="cancellationToken">(optional) cancellation token</param>
        /// <returns>enumeration for category which best matches</returns>
        public static Task<EnumT> ClassifyAsync<EnumT>(this object source, ChatClient model, string? instructions = null, CancellationToken cancellationToken = default)
            where EnumT : struct, Enum
            => source.TransformItemAsync<EnumT>(model, "classify from enum", instructions, cancellationToken);

        /// <summary>
        /// Classify source by list of categories using AI model
        /// </summary>
        /// <param name="source">source to process</param>
        /// <param name="model">ChatClient for model</param>
        /// <param name="categories">collection of categories</param>
        /// <param name="instructions">(OPTIONAL) extend instructions.</param>
        /// <param name="cancellationToken">CancellationToken</param>
        /// <returns>string from categories which best matches.</returns>
        public static Task<string> ClassifyAsync(this object source, ChatClient model, IList<string> categories, string? instructions = null, CancellationToken cancellationToken = default)
            => source.TransformItemAsync<string>(model, $"classify from categories: [{String.Join(",", categories)}]", instructions, cancellationToken);

        public static IList<ClassifiedItem<string, EnumT>> Classify<EnumT>(this IEnumerable<string> source, ChatClient model, string? instructions = null, int? maxParallel = null, CancellationToken cancellationToken = default)
            where EnumT : struct, Enum
            => source.Classify<string, EnumT>(model, instructions, maxParallel, cancellationToken);

        /// <summary>
        /// Classify collection of items using Enum and AI model
        /// </summary>
        /// <typeparam name="EnumT">enumeration to use for categories</typeparam>
        /// <param name="source">collection of text to classifiy</param>
        /// <param name="model">ChatClient for model</param>
        /// <param name="instructions">(OPTIONAL) extend instructions.</param>
        /// <param name="maxParallel">(OPTIONAL) max paralell queries to make</param>
        /// <param name="cancellationToken">CancellationToken</param>
        /// <returns>list of classifications</returns>
        public static IList<ClassifiedItem<ItemT, EnumT>> Classify<ItemT, EnumT>(this IEnumerable<ItemT> source, ChatClient model, string? instructions = null, int? maxParallel = null, CancellationToken cancellationToken = default)
            where EnumT : struct, Enum
            => source.SelectParallelAsync(async (item, index, ct) =>
            {
                var category = await item!.ClassifyAsync<EnumT>(model, instructions, ct);
                return new ClassifiedItem<ItemT, EnumT>()
                {
                    Item = item,
                    Category = category
                };
            }, maxParallel: maxParallel ?? 2 * Environment.ProcessorCount, cancellationToken: cancellationToken);

        /// <summary>
        /// Classify collection of text using collection of categories and AI model
        /// </summary>
        /// <param name="source">collection of text to classifiy</param>
        /// <param name="model">ChatClient for model</param>
        /// <param name="categories">categories to use</param>
        /// <param name="instructions">(OPTIONAL) extend instructions.</param>
        /// <param name="maxParallel">(OPTIONAL) max paralell queries to make</param>
        /// <param name="cancellationToken">CancellationToken</param>
        /// <returns>list of classifications</returns>
        public static IList<ClassifiedItem<ItemT, string>> Classify<ItemT>(this IEnumerable<ItemT> source, ChatClient model, IList<string> categories, string? goal = null, string? instructions = null, int? maxParallel = null, CancellationToken cancellationToken = default)
            => source.SelectParallelAsync(async (item, index, ct) =>
            {
                var category = await item!.ClassifyAsync(model, categories, instructions, ct);
                return new ClassifiedItem<ItemT, string>()
                {
                    Item = item,
                    Category = category
                };
            }, maxParallel: maxParallel ?? 2 * Environment.ProcessorCount, cancellationToken: cancellationToken);
    }
}

