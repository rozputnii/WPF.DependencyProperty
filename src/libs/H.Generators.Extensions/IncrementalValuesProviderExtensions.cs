﻿using H.Generators.Extensions.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Global

namespace H.Generators.Extensions;

/// <summary>
/// 
/// </summary>
public static class IncrementalValuesProviderExtensions
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="context"></param>
    /// <param name="source"></param>
    public static void AddSource(
        this IncrementalValuesProvider<FileWithName> source,
        IncrementalGeneratorInitializationContext context)
    {
        context.RegisterSourceOutput(source, static (context, file) =>
        {
            if (file.IsEmpty)
            {
                return;
            }

            context.AddSource(
                hintName: file.Name,
                source: file.Text);
        });
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="context"></param>
    /// <param name="source"></param>
    public static void AddSource(
        this IncrementalValueProvider<FileWithName> source,
        IncrementalGeneratorInitializationContext context)
    {
        context.RegisterSourceOutput(source, static (context, file) =>
        {
            if (file.IsEmpty)
            {
                return;
            }

            context.AddSource(
                hintName: file.Name,
                source: file.Text);
        });
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="context"></param>
    /// <param name="source"></param>
    public static void AddSource(
        this IncrementalValueProvider<EquatableArray<FileWithName>> source,
        IncrementalGeneratorInitializationContext context)
    {
        source
            .SelectMany(static (x, _) => x)
            .AddSource(context);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="context"></param>
    /// <param name="source"></param>
    public static void AddSource(
        this IncrementalValuesProvider<EquatableArray<FileWithName>> source,
        IncrementalGeneratorInitializationContext context)
    {
        source
            .SelectMany(static (x, _) => x)
            .AddSource(context);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="source"></param>
    public static IncrementalValueProvider<EquatableArray<TSource>> CollectAsEquatableArray<TSource>(
        this IncrementalValuesProvider<TSource> source)
        where TSource : IEquatable<TSource>
    {
        return source
            .Collect()
            .Select(static (x, _) => x.AsEquatableArray());
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="source"></param>
    /// <param name="selector"></param>
    /// <param name="initializationContext"></param>
    /// <param name="id"></param>
    /// <typeparam name="TSource"></typeparam>
    /// <typeparam name="TResult"></typeparam>
    /// <returns></returns>
    public static IncrementalValueProvider<TResult> SelectAndReportExceptions<TSource, TResult>(
        this IncrementalValueProvider<TSource> source,
        Func<TSource, CancellationToken, TResult> selector,
        IncrementalGeneratorInitializationContext initializationContext,
        string id = "SRE001")
    {
        var outputWithErrors = source
            .Select<TSource, (TResult? Value, Exception? Exception)>((value, cancellationToken) =>
            {
                cancellationToken.ThrowIfCancellationRequested();

                try
                {
                    return (Value: selector(value, cancellationToken), Exception: null);
                }
                catch (Exception exception)
                {
                    return (Value: default, Exception: exception);
                }
            });

        initializationContext.RegisterSourceOutput(outputWithErrors,
            (context, tuple) =>
            {
                if (tuple.Exception is null)
                {
                    return;
                }

                context.ReportException(id: id, exception: tuple.Exception);
            });

        return outputWithErrors
            .Select(static (x, _) => x.Value!);
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="source"></param>
    /// <param name="initializationContext"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static IncrementalValuesProvider<T> SelectAndReportDiagnostics<T>(
        this IncrementalValuesProvider<ResultWithDiagnostics<T?>> source,
        IncrementalGeneratorInitializationContext initializationContext)
    {
        initializationContext.RegisterSourceOutput(
            source.SelectMany(static (x, _) => x.Diagnostics),
            static (context, diagnostic) => context.ReportDiagnostic(diagnostic));

        return source
            .Where(static x => x.Result is not null)
            .Select(static (x, _) => x.Result!);
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="source"></param>
    /// <param name="initializationContext"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static IncrementalValueProvider<T?> SelectAndReportDiagnostics<T>(
        this IncrementalValueProvider<ResultWithDiagnostics<T?>> source,
        IncrementalGeneratorInitializationContext initializationContext)
    {
        initializationContext.RegisterSourceOutput(
            source.SelectMany(static (x, _) => x.Diagnostics),
            static (context, diagnostic) => context.ReportDiagnostic(diagnostic));

        return source
            .Select(static (x, _) => x.Result);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="source"></param>
    /// <param name="selector"></param>
    /// <param name="initializationContext"></param>
    /// <param name="id"></param>
    /// <typeparam name="TSource"></typeparam>
    /// <typeparam name="TResult"></typeparam>
    /// <returns></returns>
    public static IncrementalValuesProvider<TResult> SelectAndReportExceptions<TSource, TResult>(
        this IncrementalValuesProvider<TSource> source,
        Func<TSource, CancellationToken, TResult> selector,
        IncrementalGeneratorInitializationContext initializationContext,
        string id = "SRE001")
    {
        var outputWithErrors = source
            .Select<TSource, (TResult? Value, Exception? Exception)>((value, cancellationToken) =>
            {
                cancellationToken.ThrowIfCancellationRequested();

                try
                {
                    return (Value: selector(value, cancellationToken), Exception: null);
                }
                catch (Exception exception)
                {
                    return (Value: default, Exception: exception);
                }
            });

        initializationContext.RegisterSourceOutput(outputWithErrors
            .Where(static x => x.Exception is not null),
            (context, tuple) =>
            {
                context.ReportException(id: id, exception: tuple.Exception!);
            });

        return outputWithErrors
            .Where(static x => x.Exception is null)
            .Select(static (x, _) => x.Value!);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="source"></param>
    /// <param name="selector"></param>
    /// <param name="initializationContext"></param>
    /// <param name="id"></param>
    /// <typeparam name="TSource"></typeparam>
    /// <typeparam name="TResult"></typeparam>
    /// <returns></returns>
    public static IncrementalValuesProvider<TResult> SelectAndReportExceptions<TSource, TResult>(
        this IncrementalValuesProvider<TSource> source,
        Func<TSource, TResult> selector,
        IncrementalGeneratorInitializationContext initializationContext,
        string id = "SRE001")
    {
        return source
            .SelectAndReportExceptions((x, _) => selector(x), initializationContext, id);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="source"></param>
    /// <param name="selector"></param>
    /// <param name="initializationContext"></param>
    /// <param name="id"></param>
    /// <typeparam name="TSource"></typeparam>
    /// <typeparam name="TResult"></typeparam>
    /// <returns></returns>
    public static IncrementalValueProvider<TResult> SelectAndReportExceptions<TSource, TResult>(
        this IncrementalValueProvider<TSource> source,
        Func<TSource, TResult> selector,
        IncrementalGeneratorInitializationContext initializationContext,
        string id = "SRE001")
    {
        return source
            .SelectAndReportExceptions((x, _) => selector(x), initializationContext, id);
    }
    
    /// <summary>
    /// Filters nullable values and select non-nullable values.
    /// </summary>
    /// <param name="source"></param>
    /// <typeparam name="TSource"></typeparam>
    /// <returns></returns>
    public static IncrementalValuesProvider<TSource> WhereNotNull<TSource>(
        this IncrementalValuesProvider<TSource?> source)
        where TSource : struct
    {
        return source
            .Where(static x => x is not null)
            .Select(static (x, _) => x!.Value);
    }

}
