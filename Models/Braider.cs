using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace AsinoPuzzles.Functions.Models
{
    public sealed class BraiderSummary {
        public string Id { get; set; }
        public string Title { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime DateUpdated { get; set; }

        public BraiderSummary(Braider braider)
        {
            Id = braider.Id;
            Title = braider.Title;
            DateCreated = braider.DateCreated;
            DateUpdated = braider.DateUpdated;
        }
    }

    public sealed class Braider {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }
        [JsonProperty(PropertyName = "partitionKey")]
        public string PartitionKey { get; set; }
        public string UserId { get; set; }
        public string Title { get; set; }
        public List<BraiderVariable> Variables { get; set; }
        public List<BraiderElement> Elements { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime DateUpdated { get; set; }
        public bool IsDeleted { get; set; }
    }

    public sealed class BraiderResult {
        public string Id { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string Title { get; set; }
        public List<BraiderVariable> Variables { get; set; }
        public List<BraiderElement> Elements { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime DateUpdated { get; set; }

        public BraiderResult(Braider braider, User user)
        {
            Id = braider.Id;
            UserId = user.Id;
            UserName = user.Name;
            Title = braider.Title;
            Variables = braider.Variables;
            Elements = braider.Elements;
            DateCreated = braider.DateCreated;
            DateUpdated = braider.DateUpdated;
        }
    }

    public sealed class BraiderStyle {
        public string FontWeight { get; set; }
        public string FontStyle { get; set; }
    }

    public sealed class BraiderSpan {
        public string Type { get; set; }
        public List<BraiderSpan> Spans { get; set; }
        public BraiderStyle Style { get; set; }
        public string IsVariableId { get; set; }
        public string PageId { get; set; }
        public string Value { get; set; }
        public string VariableId { get; set; }
    }

    public sealed class BraiderSelectOptionString {
        public string Id { get; set; }
        public List<BraiderSpan> Spans { get; set; }
        public string IsVariableId { get; set; }
    }

    public sealed class BraiderVariable {
        public string Id { get; set; }
        public string Description { get; set; }
        public string Type { get; set; }
        public string Format { get; set; }
        public string Expression { get; set; }
        public string VariableId { get; set; }
        public string DefaultOptionId { get; set; }
        public string DefaultValue { get; set; }
        public List<BraiderSelectOptionString> Options { get; set; }
        public string OptionId { get; set; }
        public string Value { get; set; }
    }

    public sealed class BraiderElement {
        public string Id { get; set; }
        public string Description { get; set; }
        public string Type { get; set; }
        public List<BraiderSpan> Spans { get; set; }
        public string VariableId { get; set; }
        public string IsVariableId { get; set; }
    }

    public static class BraiderFunctions {
        public static List<BraiderSpan> ValidateSpans(List<BraiderSpan> spans, List<BraiderVariable> variables) {
            var validatedSpans = new List<BraiderSpan>();

            spans?.ForEach(span => {
                var validatedSpan = new BraiderSpan();

                validatedSpan.IsVariableId = span.IsVariableId;

                if (!string.IsNullOrEmpty(validatedSpan.IsVariableId) && !variables.Exists(v => v.Format == "BOOLEAN" && v.Id == validatedSpan.IsVariableId))
                    throw new System.ArgumentException("Span must have a valid is variable id");

                validatedSpan.Type = span.Type;

                if (validatedSpan.Type == "TEXT") {
                    validatedSpan.Value = span.Value;
                } else if (validatedSpan.Type == "VARIABLE") {
                    validatedSpan.VariableId = span.VariableId;

                    if (!string.IsNullOrEmpty(validatedSpan.VariableId) && !variables.Exists(v => v.Format == "TEXT" && v.Id == validatedSpan.VariableId))
                        throw new System.ArgumentException("Span must have a valid variable id");
                } else if (validatedSpan.Type == "GROUP") {

                } else {
                    throw new System.ArgumentException("Each span must have a valid type");
                }

                validatedSpans.Add(validatedSpan);
            });

            return validatedSpans;
        }

        public static List<BraiderElement> ValidateElements(List<BraiderElement> elements, List<BraiderVariable> variables) {
            var validatedElements = new List<BraiderElement>();

            elements?.ForEach(element => {
                var validatedElement = new BraiderElement();

                if (string.IsNullOrEmpty(element.Id))
                    throw new System.ArgumentException("Each element must have an id");

                if (validatedElements.Exists(validatedElement => element.Id == validatedElement.Id))
                    throw new System.ArgumentException("Each element id must be unique");

                if (string.IsNullOrEmpty(element.Description))
                    throw new System.ArgumentException("Each element must have a description");

                validatedElement.Id = element.Id;
                validatedElement.Description = element.Description;
                validatedElement.Type = element.Type;
                validatedElement.IsVariableId = element.IsVariableId;

                if (!string.IsNullOrEmpty(validatedElement.IsVariableId) && !variables.Exists(v => v.Format == "BOOLEAN" && v.Id == validatedElement.IsVariableId))
                    throw new System.ArgumentException("Element must have a valid is variable id");

                if (validatedElement.Type == "PARAGRAPH" || validatedElement.Type == "HEADING_2") {
                    validatedElement.Spans = element.Spans != null ? ValidateSpans(element.Spans, variables) : null;
                } else if (validatedElement.Type == "INPUT") {
                    validatedElement.VariableId = element.VariableId;

                    if (!variables.Exists(v => v.Type == "INPUT" && v.Id == validatedElement.VariableId))
                        throw new System.ArgumentException("Input element must have a valid variable id");
                } else if (validatedElement.Type == "GROUP") {

                } else {
                    throw new System.ArgumentException("Each element must have a valid type");
                }

                validatedElements.Add(validatedElement);
            });

            return validatedElements;
        }

        public static List<BraiderVariable> ValidateVariables(List<BraiderVariable> variables) {
            var validatedVariables = new List<BraiderVariable>();

            variables?.ForEach(variable => {
                var validatedVariable = new BraiderVariable();

                if (string.IsNullOrEmpty(variable.Id))
                    throw new System.ArgumentException("Each variable must have an id");

                if (validatedVariables.Exists(validatedVariable => validatedVariable.Id == variable.Id))
                    throw new System.ArgumentException("Each variable id must be unique");

                validatedVariable.Id = variable.Id;
                validatedVariable.Format = variable.Format;
                validatedVariable.Type = variable.Type;

                if (validatedVariable.Type == "INPUT") {
                    validatedVariable.Description = variable.Description;

                    if (string.IsNullOrEmpty(validatedVariable.Description))
                        throw new System.ArgumentException("Each input variable must have a description");
                } else {
                    validatedVariable.Expression = variable.Expression;

                    if (validatedVariable.Expression == null)
                        throw new System.ArgumentException("Each evaluated or system variable must have an expression");
                }

                if (validatedVariable.Format == "TEXT") {
                    if (validatedVariable.Type == "INPUT") {
                        if (variable.Options == null) {
                            validatedVariable.DefaultValue = variable.DefaultValue;

                            if (validatedVariable.DefaultValue == null)
                              throw new System.ArgumentException("Each text input select variable must have a default option");
                        } else {

                        }
                    } else {
                      throw new System.ArgumentException("Each text variable must have a valid type");
                    }
                } else if (validatedVariable.Format == "NUMBER") {

                } else if (validatedVariable.Format == "BOOLEAN") {
                    if (validatedVariable.Type == "SYSTEM") {
                        if (validatedVariable.Expression == "IS_VARIABLE_SET") {
                            validatedVariable.VariableId = variable.VariableId;

                            if (!variables.Exists(v => v.Type == "INPUT" && v.Id == validatedVariable.VariableId))
                                throw new System.ArgumentException("Each is variable set variable must have a valid variable id");
                        } else if (validatedVariable.Expression == "IS_VARIABLE_NOT_SET") {
                            validatedVariable.VariableId = variable.VariableId;

                            if (!variables.Exists(v => v.Type == "INPUT" && v.Id == validatedVariable.VariableId))
                                throw new System.ArgumentException("Each is variable not set variable must have a valid variable id");
                        }
                    } else {

                    }
                } else {
                  throw new System.ArgumentException("Each variable must have a valid format");
                }

                validatedVariables.Add(validatedVariable);
            });

            return validatedVariables;
        }
    }
}
