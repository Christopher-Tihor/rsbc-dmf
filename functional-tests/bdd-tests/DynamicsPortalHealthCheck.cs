﻿using OpenQA.Selenium;
using Protractor;
using System;
using System.Threading;
using Xunit;
using Xunit.Gherkin.Quick;

/*
Feature: DynamicsPortalHealthCheck
    As a medical professional
    I want to perform a health check on the Dynamics portal

@pipeline
Scenario: Dynamics Portal Authentication
    When I log in to the Dynamics portal
    And I wait for the Dynamics homepage content to be displayed
    Then I log out of the portal
*/

namespace bdd_tests
{
    [FeatureFile("./DynamicsPortalHealthCheck.feature")]
    public sealed class DynamicsPortalHealthCheck : TestBase
    {
    }
}