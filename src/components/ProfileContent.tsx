"use client";

import { AuthenticatedTemplate, UnauthenticatedTemplate, useMsal } from "@azure/msal-react";
import { SignInButton, SignOutButton } from "./AuthButtons";
import { useEffect, useState } from "react";

interface UserInfo {
  name?: string;
  email?: string;
  id?: string;
  givenName?: string;
  surname?: string;
  jobTitle?: string;
  displayName?: string;
}

export const ProfileContent = () => {
  const { accounts } = useMsal();
  const [userInfo, setUserInfo] = useState<UserInfo | null>(null);

  const account = accounts[0];

  useEffect(() => {
    if (account) {
      // Extract user information from the account claims
      const claims = account.idTokenClaims as Record<string, unknown>;
      setUserInfo({
        name: (claims?.name as string) || account.name,
        email: (claims?.email as string) || (claims?.preferred_username as string) || account.username,
        id: (claims?.oid as string) || (claims?.sub as string),
        givenName: claims?.given_name as string,
        surname: claims?.family_name as string,
        jobTitle: claims?.jobTitle as string,
        displayName: (claims?.name as string) || `${(claims?.given_name as string) || ''} ${(claims?.family_name as string) || ''}`.trim(),
      });
    }
  }, [account]);

  return (
    <div className="min-h-screen bg-gradient-to-br from-blue-50 to-indigo-100 flex items-center justify-center py-12 px-4 sm:px-6 lg:px-8">
      <div className="max-w-md w-full space-y-8">
        <div className="text-center">
          <h1 className="text-4xl font-bold text-gray-900 mb-2">EFSM</h1>
          <p className="text-gray-600">Azure AD Authentication Demo</p>
        </div>

        <AuthenticatedTemplate>
          <div className="bg-white rounded-lg shadow-xl p-8 space-y-6">
            <div className="text-center">
              <div className="w-20 h-20 bg-blue-500 rounded-full mx-auto mb-4 flex items-center justify-center">
                <span className="text-white text-2xl font-bold">
                  {userInfo?.displayName?.charAt(0) || userInfo?.name?.charAt(0) || "U"}
                </span>
              </div>
              <h2 className="text-2xl font-bold text-gray-900 mb-4">
                Welcome back, {userInfo?.displayName || userInfo?.name || "User"}!
              </h2>
              
              <p className="text-gray-600 mb-6">
                You are successfully logged in to EFSM.
              </p>

              {/* Quick Info */}
              <div className="bg-gray-50 p-4 rounded-lg mb-6">
                <p className="text-sm text-gray-600 mb-1">
                  <strong>Email:</strong> {userInfo?.email || "Not available"}
                </p>
                <p className="text-sm text-gray-600">
                  <strong>Account Type:</strong> {account?.tenantId ? 'Work/School Account' : 'Personal Account'}
                </p>
              </div>

              {/* Action Buttons */}
              <div className="flex flex-col sm:flex-row gap-4 justify-center">
                <a
                  href="/profile"
                  className="bg-green-600 hover:bg-green-700 text-white font-semibold py-3 px-6 rounded-lg shadow-md transition duration-300 ease-in-out transform hover:scale-105 focus:outline-none focus:ring-2 focus:ring-green-500 focus:ring-opacity-75 flex items-center justify-center"
                >
                  <svg className="w-5 h-5 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z" />
                  </svg>
                  View Full Profile
                </a>
                <SignOutButton />
              </div>
            </div>
          </div>
        </AuthenticatedTemplate>

        <UnauthenticatedTemplate>
          <div className="bg-white rounded-lg shadow-xl p-8 space-y-6">
            <div className="text-center">
              <div className="w-20 h-20 bg-gray-300 rounded-full mx-auto mb-4 flex items-center justify-center">
                <svg
                  className="w-10 h-10 text-gray-500"
                  fill="none"
                  stroke="currentColor"
                  viewBox="0 0 24 24"
                >
                  <path
                    strokeLinecap="round"
                    strokeLinejoin="round"
                    strokeWidth={2}
                    d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z"
                  />
                </svg>
              </div>
              <h2 className="text-2xl font-bold text-gray-900 mb-2">
                Welcome to EFSM
              </h2>
              <p className="text-gray-600 mb-6">
                Please sign in with your Microsoft account to continue
              </p>
            </div>
            
            <div className="text-center">
              <SignInButton />
            </div>
            
            <div className="text-center text-sm text-gray-500">
              <p>Secure authentication powered by Microsoft Azure AD</p>
            </div>
          </div>
        </UnauthenticatedTemplate>
      </div>
    </div>
  );
};