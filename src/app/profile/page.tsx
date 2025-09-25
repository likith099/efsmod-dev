"use client";

import { AuthenticatedTemplate, UnauthenticatedTemplate, useMsal } from "@azure/msal-react";
import { useEffect, useState } from "react";
import { useRouter } from "next/navigation";

interface UserInfo {
  name?: string;
  email?: string;
  id?: string;
  givenName?: string;
  surname?: string;
  jobTitle?: string;
  displayName?: string;
}

export default function ProfilePage() {
  const { accounts, instance } = useMsal();
  const [userInfo, setUserInfo] = useState<UserInfo | null>(null);
  const router = useRouter();
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

  const handleSignOut = async () => {
    try {
      await instance.logoutRedirect({
        postLogoutRedirectUri: window.location.origin,
      });
    } catch (error) {
      console.error("Logout failed:", error);
    }
  };

  const handleBackToHome = () => {
    router.push('/');
  };

  return (
    <div className="min-h-screen bg-gradient-to-br from-blue-50 to-indigo-100 py-12 px-4 sm:px-6 lg:px-8">
      <div className="max-w-4xl mx-auto">
        <AuthenticatedTemplate>
          <div className="bg-white rounded-lg shadow-xl overflow-hidden">
            {/* Header */}
            <div className="bg-gradient-to-r from-blue-600 to-indigo-700 px-6 py-8">
              <div className="flex items-center space-x-4">
                <div className="w-20 h-20 bg-white/20 rounded-full flex items-center justify-center">
                  <span className="text-white text-3xl font-bold">
                    {userInfo?.displayName?.charAt(0) || userInfo?.name?.charAt(0) || "U"}
                  </span>
                </div>
                <div>
                  <h1 className="text-3xl font-bold text-white">
                    {userInfo?.displayName || userInfo?.name || "Welcome!"}
                  </h1>
                  <p className="text-blue-100 mt-1">
                    {userInfo?.email || "User Profile"}
                  </p>
                </div>
              </div>
            </div>

            {/* Content */}
            <div className="p-6">
              <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                {/* Personal Information */}
                <div className="bg-gray-50 rounded-lg p-6">
                  <h2 className="text-xl font-semibold text-gray-900 mb-4 flex items-center">
                    <svg className="w-5 h-5 mr-2 text-blue-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z" />
                    </svg>
                    Personal Information
                  </h2>
                  <div className="space-y-4">
                    {userInfo?.displayName && (
                      <div className="flex justify-between border-b pb-2">
                        <span className="font-medium text-gray-600">Display Name:</span>
                        <span className="text-gray-900">{userInfo.displayName}</span>
                      </div>
                    )}
                    
                    {userInfo?.givenName && (
                      <div className="flex justify-between border-b pb-2">
                        <span className="font-medium text-gray-600">First Name:</span>
                        <span className="text-gray-900">{userInfo.givenName}</span>
                      </div>
                    )}

                    {userInfo?.surname && (
                      <div className="flex justify-between border-b pb-2">
                        <span className="font-medium text-gray-600">Last Name:</span>
                        <span className="text-gray-900">{userInfo.surname}</span>
                      </div>
                    )}

                    {userInfo?.email && (
                      <div className="flex justify-between border-b pb-2">
                        <span className="font-medium text-gray-600">Email:</span>
                        <span className="text-gray-900 break-all">{userInfo.email}</span>
                      </div>
                    )}

                    {userInfo?.jobTitle && (
                      <div className="flex justify-between border-b pb-2">
                        <span className="font-medium text-gray-600">Job Title:</span>
                        <span className="text-gray-900">{userInfo.jobTitle}</span>
                      </div>
                    )}
                  </div>
                </div>

                {/* Account Information */}
                <div className="bg-gray-50 rounded-lg p-6">
                  <h2 className="text-xl font-semibold text-gray-900 mb-4 flex items-center">
                    <svg className="w-5 h-5 mr-2 text-green-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z" />
                    </svg>
                    Account Details
                  </h2>
                  <div className="space-y-4">
                    <div className="flex justify-between border-b pb-2">
                      <span className="font-medium text-gray-600">Account Type:</span>
                      <span className="text-gray-900">
                        {account?.tenantId ? 'Work/School Account' : 'Personal Account'}
                      </span>
                    </div>

                    {userInfo?.id && (
                      <div className="flex justify-between border-b pb-2">
                        <span className="font-medium text-gray-600">User ID:</span>
                        <span className="text-gray-900 text-sm font-mono truncate ml-2" title={userInfo.id}>
                          {userInfo.id.substring(0, 20)}...
                        </span>
                      </div>
                    )}

                    {account?.tenantId && (
                      <div className="flex justify-between border-b pb-2">
                        <span className="font-medium text-gray-600">Tenant ID:</span>
                        <span className="text-gray-900 text-sm font-mono">
                          EFSM ({account.tenantId.substring(0, 8)}...)
                        </span>
                      </div>
                    )}

                    <div className="flex justify-between border-b pb-2">
                      <span className="font-medium text-gray-600">Login Time:</span>
                      <span className="text-gray-900">
                        {new Date().toLocaleString()}
                      </span>
                    </div>
                  </div>
                </div>
              </div>

              {/* Security Information */}
              <div className="mt-6 bg-blue-50 rounded-lg p-6">
                <h3 className="text-lg font-semibold text-blue-900 mb-3 flex items-center">
                  <svg className="w-5 h-5 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 15v2m-6 4h12a2 2 0 002-2v-6a2 2 0 00-2-2H6a2 2 0 00-2 2v6a2 2 0 002 2zm10-10V7a4 4 0 00-8 0v4h8z" />
                  </svg>
                  Authentication Status
                </h3>
                <div className="flex items-center space-x-2">
                  <div className="w-3 h-3 bg-green-500 rounded-full animate-pulse"></div>
                  <span className="text-green-800 font-medium">Successfully authenticated via EFSM Azure AD</span>
                </div>
              </div>

              {/* Action Buttons */}
              <div className="mt-8 flex flex-col sm:flex-row gap-4 justify-center">
                <button
                  onClick={handleBackToHome}
                  className="bg-blue-600 hover:bg-blue-700 text-white font-semibold py-3 px-6 rounded-lg shadow-md transition duration-300 ease-in-out transform hover:scale-105 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:ring-opacity-75 flex items-center justify-center"
                >
                  <svg className="w-5 h-5 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M3 12l2-2m0 0l7-7 7 7M5 10v10a1 1 0 001 1h3m10-11l2 2m-2-2v10a1 1 0 01-1 1h-3m-6 0a1 1 0 001-1v-4a1 1 0 011-1h2a1 1 0 011 1v4a1 1 0 001 1m-6 0h6" />
                  </svg>
                  Back to Home
                </button>
                
                <button
                  onClick={handleSignOut}
                  className="bg-red-600 hover:bg-red-700 text-white font-semibold py-3 px-6 rounded-lg shadow-md transition duration-300 ease-in-out transform hover:scale-105 focus:outline-none focus:ring-2 focus:ring-red-500 focus:ring-opacity-75 flex items-center justify-center"
                >
                  <svg className="w-5 h-5 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M17 16l4-4m0 0l-4-4m4 4H7m6 4v1a3 3 0 01-3 3H6a3 3 0 01-3-3V7a3 3 0 013-3h4a3 3 0 013 3v1" />
                  </svg>
                  Sign Out
                </button>
              </div>
            </div>
          </div>
        </AuthenticatedTemplate>

        <UnauthenticatedTemplate>
          <div className="bg-white rounded-lg shadow-xl p-8 text-center">
            <div className="w-20 h-20 bg-red-100 rounded-full mx-auto mb-4 flex items-center justify-center">
              <svg className="w-10 h-10 text-red-500" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-2.5L13.732 4c-.77-.833-1.664-.833-2.464 0L4.35 16.5c-.77.833.192 2.5 1.732 2.5z" />
              </svg>
            </div>
            <h2 className="text-2xl font-bold text-gray-900 mb-2">Access Denied</h2>
            <p className="text-gray-600 mb-6">You need to be signed in to view this profile page.</p>
            <button
              onClick={handleBackToHome}
              className="bg-blue-600 hover:bg-blue-700 text-white font-semibold py-2 px-4 rounded-lg"
            >
              Go to Home
            </button>
          </div>
        </UnauthenticatedTemplate>
      </div>
    </div>
  );
}