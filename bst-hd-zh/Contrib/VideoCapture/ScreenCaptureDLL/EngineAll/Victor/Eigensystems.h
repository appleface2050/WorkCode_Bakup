#ifndef _RNP_EIGENSYSTEMS_H_
#define _RNP_EIGENSYSTEMS_H_

#include <cstddef>
#include <complex>

namespace RNP{

// Eigensystem
// ===========
// Solves an eigensystem:
//   A * X = X * lambda
// NOTE: This interface is NOT FINAL.
//
// Arguments:
//   n         Dimension of the problem.
//   a         The matrix a. Overwritten on exit.
//   lda       Leading dimension of matrix a.
//   eval      Eigenvalues are returned here.
//   evec      Eigenvectors are returned here.
//   ldvec     Leading dimension of matrix evec.
//   max_iter  Maximum number of iterations to take.
//   nrot      Number of Jacobi rotations returned here.
int Eigensystem(
	size_t n, std::complex<double>* a, size_t lda,
	std::complex<double>* eval,
	std::complex<double>* evec, size_t ldvec,
	unsigned int max_iter, unsigned int *nrot);

}; // namespace RNP

#endif // _RNP_EIGENSYSTEMS_H_
